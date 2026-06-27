using JwtMusic.WebUI.Dtos.SongDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace JwtMusic.WebUI.Controllers
{
    public class SongController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SongController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
            => _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

        public async Task<IActionResult> SongList()
        {
            var token = GetToken();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:7185/api/Songs");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Login");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("SingIn", "Login");

            var json = await response.Content.ReadAsStringAsync();
            var songs = JsonConvert.DeserializeObject<List<ResultSongDto>>(json) ?? new List<ResultSongDto>();

            ViewBag.Token = token;

            return View(songs);
        }

    }
}
