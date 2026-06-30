using JwtMusic.WebUI.Dtos.LikedSong;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace JwtMusic.WebUI.Controllers
{

    public class LikedSongsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LikedSongsController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
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
        // Index — Beğenilen Şarkıları Sunucu Tarafında Çeker ve View'a Gönderir
        // ─────────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetToken();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var responseMessage = await client.GetAsync("https://localhost:7185/api/LikedSongs");

            if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Login");

            if (!responseMessage.IsSuccessStatusCode)
                return RedirectToAction("SingIn", "Login");

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<ResultLikedSongDto>>(jsonData) ?? new List<ResultLikedSongDto>();

            ViewBag.Token = token;

            // HATA ÇÖZÜMÜ: Çakışmayı önlemek için SongDtos altındaki ResultSongDto'yu tam adıyla (Full Namespace) çağırıyoruz
            var mappedSongs = new List<JwtMusic.WebUI.Dtos.SongDtos.ResultSongDto>();
            foreach (var item in values)
            {
                mappedSongs.Add(new JwtMusic.WebUI.Dtos.SongDtos.ResultSongDto
                {
                    SongId = item.SongId,
                    Title = item.Title,
                    CoverImageUrl = item.ImageUrl,
                    ArtistName = item.ArtistName,
                    RequiredRoleName = "Basic"
                });
            }

            ViewBag.SongsJson = JsonConvert.SerializeObject(mappedSongs);

            return View(values);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // ToggleLike — JavaScript/Ajax İstekleri İçin Güvenli UI Köprüsü
        // ─────────────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Oturum bulunamadı veya süresi doldu." });
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var responseMessage = await client.PostAsync($"https://localhost:7185/api/LikedSongs/toggle/{id}", null);

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UpdateLikedSongToggleDto>(jsonData);
                return Json(result);
            }

            return BadRequest(new { message = "Beğeni senkronizasyon işlemi başarısız oldu." });
        }
    }
}
