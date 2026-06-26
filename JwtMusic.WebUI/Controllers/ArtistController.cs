using JwtMusic.WebUI.Dtos;
using JwtMusic.WebUI.Dtos.SongDtos;
using JwtMusic.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace JwtMusic.WebUI.Controllers
{
    public class ArtistController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ArtistController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> ArtistList()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Sanatçıları Çekiyoruz
            var response = await client.GetAsync("https://localhost:7185/api/Artist");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultArtistDto>>(jsonData);

                // 🌟 [EKLEME]: Kullanıcının takip ettiği sanatçı ID'lerini API'den çekiyoruz
                var followResponse = await client.GetAsync("https://localhost:7185/api/Follows/my-follows");
                var followedArtistIds = new List<int>();

                if (followResponse.IsSuccessStatusCode)
                {
                    var jsonFollows = await followResponse.Content.ReadAsStringAsync();
                    followedArtistIds = JsonConvert.DeserializeObject<List<int>>(jsonFollows) ?? new List<int>();
                }

                // View tarafında buton durumunu (Takip Et/Takipten Çık) kontrol etmek için listeyi yolluyoruz
                ViewBag.FollowedArtistIds = followedArtistIds;

                return View(values);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            return RedirectToAction("SingIn", "Login");
        }

        public async Task<IActionResult> ArtistDetail(int id)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Sanatçı verisini çekiyoruz
            var responseArtist = await client.GetAsync($"https://localhost:7185/api/Artist/{id}");

            if (responseArtist.IsSuccessStatusCode)
            {
                var jsonArtist = await responseArtist.Content.ReadAsStringAsync();
                var artistData = JsonConvert.DeserializeObject<ResultArtistDto>(jsonArtist);

                // 2. ÇÖZÜM: Tüm şarkıları api/Songs adresinden çekiyoruz
                var responseSongs = await client.GetAsync("https://localhost:7185/api/Songs");
                var songList = new List<ResultSongDto>();

                if (responseSongs.IsSuccessStatusCode)
                {
                    var jsonSongs = await responseSongs.Content.ReadAsStringAsync();
                    var allSongs = JsonConvert.DeserializeObject<List<ResultSongDto>>(jsonSongs);

                    // 3. Çekilen tüm şarkılardan sadece bu sanatçıya (ArtistId) ait olanları filtreliyoruz
                    if (allSongs != null)
                    {
                        songList = allSongs.Where(x => x.ArtistId == id).ToList();
                    }
                }

                // 🌟 [EKLEME]: Detay sayfasında da buton durumunu korumak için kullanıcının takip listesini alıyoruz
                var followResponse = await client.GetAsync("https://localhost:7185/api/Follows/my-follows");
                var followedArtistIds = new List<int>();

                if (followResponse.IsSuccessStatusCode)
                {
                    var jsonFollows = await followResponse.Content.ReadAsStringAsync();
                    followedArtistIds = JsonConvert.DeserializeObject<List<int>>(jsonFollows) ?? new List<int>();
                }

                ViewBag.FollowedArtistIds = followedArtistIds;

                var viewModel = new ArtistDetailViewModel
                {
                    Artist = artistData,
                    Songs = songList
                };

                return View(viewModel);
            }

            if (responseArtist.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            return RedirectToAction("SingIn", "Login");
        }
    }
}