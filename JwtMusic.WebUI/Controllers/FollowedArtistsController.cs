using JwtMusic.WebUI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace JwtMusic.WebUI.Controllers
{
    public class FollowedArtistsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FollowedArtistsController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
            => _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SingIn", "Login");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Takip edilen sanatçı ID'lerini çek
            var followResponse = await client.GetAsync("https://localhost:7185/api/Follows/my-follows");
            if (followResponse.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Login");
            if (!followResponse.IsSuccessStatusCode)
                return RedirectToAction("SingIn", "Login");

            var jsonFollows = await followResponse.Content.ReadAsStringAsync();
            var followedArtistIds = JsonConvert.DeserializeObject<List<int>>(jsonFollows) ?? new List<int>();

            // 2. Tüm sanatçıları çek
            var responseArtist = await client.GetAsync("https://localhost:7185/api/Artist");
            var artists = new List<ResultArtistDto>();
            if (responseArtist.IsSuccessStatusCode)
            {
                var jsonArtists = await responseArtist.Content.ReadAsStringAsync();
                artists = JsonConvert.DeserializeObject<List<ResultArtistDto>>(jsonArtists) ?? new List<ResultArtistDto>();
            }

            // 3. Sadece takip edilenleri filtrele
            var followedArtists = artists
                .Where(a => followedArtistIds.Contains(a.ArtistId))
                .ToList();

            ViewBag.FollowedArtistIds = followedArtistIds;
            ViewBag.Token = token;

            return View(followedArtists);
        }
    }
}