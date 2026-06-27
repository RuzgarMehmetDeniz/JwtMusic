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

        // ─────────────────────────────────────────────────────────────────────────
        // Yardımcı: Token'ı session'dan okur, boşsa null döner
        // ─────────────────────────────────────────────────────────────────────────
        private string? GetToken()
            => _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

        // ─────────────────────────────────────────────────────────────────────────
        // ArtistList
        // Değişiklik: Şarkıları burada (sunucu tarafında) çekip ViewBag.Songs ile
        // view'a gönderiyoruz. Böylece frontend, play'e basıldığında ayrıca bir
        // fetch() yapmak yerine sayfada zaten hazır olan veriyi kullanabilir.
        // ─────────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> ArtistList()
        {
            var token = GetToken();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Sanatçıları çek
            var responseArtist = await client.GetAsync("https://localhost:7185/api/Artist");

            if (responseArtist.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Login");

            if (!responseArtist.IsSuccessStatusCode)
                return RedirectToAction("SingIn", "Login");

            var jsonArtists = await responseArtist.Content.ReadAsStringAsync();
            var artists = JsonConvert.DeserializeObject<List<ResultArtistDto>>(jsonArtists);

            // 2. Tüm şarkıları çek → view'a göndereceğiz (frontend fetch() yapmayacak)
            var responseSongs = await client.GetAsync("https://localhost:7185/api/Songs");
            var allSongs = new List<ResultSongDto>();

            if (responseSongs.IsSuccessStatusCode)
            {
                var jsonSongs = await responseSongs.Content.ReadAsStringAsync();
                allSongs = JsonConvert.DeserializeObject<List<ResultSongDto>>(jsonSongs) ?? new List<ResultSongDto>();
            }

            // 3. Takip edilen sanatçı ID'lerini çek
            var followResponse = await client.GetAsync("https://localhost:7185/api/Follows/my-follows");
            var followedArtistIds = new List<int>();

            if (followResponse.IsSuccessStatusCode)
            {
                var jsonFollows = await followResponse.Content.ReadAsStringAsync();
                followedArtistIds = JsonConvert.DeserializeObject<List<int>>(jsonFollows) ?? new List<int>();
            }

            ViewBag.FollowedArtistIds = followedArtistIds;
            ViewBag.Token = token; // JS fetch() için token
            ViewBag.SongsJson = JsonConvert.SerializeObject(allSongs);

            return View(artists);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // ArtistDetail — değişiklik yok, aynı mantık korundu
        // ─────────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> ArtistDetail(int id)
        {
            var token = GetToken();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Sanatçı verisini çek
            var responseArtist = await client.GetAsync($"https://localhost:7185/api/Artist/{id}");

            if (responseArtist.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Login");

            if (!responseArtist.IsSuccessStatusCode)
                return RedirectToAction("SingIn", "Login");

            var jsonArtist = await responseArtist.Content.ReadAsStringAsync();
            var artistData = JsonConvert.DeserializeObject<ResultArtistDto>(jsonArtist);

            // 2. Tüm şarkıları çek, bu sanatçıya ait olanları filtrele
            var responseSongs = await client.GetAsync("https://localhost:7185/api/Songs");
            var songList = new List<ResultSongDto>();

            if (responseSongs.IsSuccessStatusCode)
            {
                var jsonSongs = await responseSongs.Content.ReadAsStringAsync();
                var allSongs = JsonConvert.DeserializeObject<List<ResultSongDto>>(jsonSongs);

                if (allSongs != null)
                    songList = allSongs.Where(x => x.ArtistId == id).ToList();
            }

            // 3. Takip listesi
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
    }
}