using JwtMusic.WebUI.Dtos;
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

            // Port 7185 olarak güncellendi
            var response = await client.GetAsync("https://localhost:7185/api/Artist");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultArtistDto>>(jsonData);
                return View(values);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            return RedirectToAction("SingIn", "Login");
        }

        [HttpGet]
        public async Task<IActionResult> GetArtistDetailPartial(int id)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Port 7185 olarak güncellendi
            var response = await client.GetAsync($"https://localhost:7185/api/Artist/GetArtistWithMusics/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<GetArtistWithMusicsDto>(jsonData);

                return PartialView("_ArtistDetailPartial", values);
            }

            return BadRequest("Sanatçı detayları yüklenemedi.");
        }

        [HttpGet]
        public async Task<IActionResult> PlayArtistTopTrack(int artistId)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "UNAUTHORIZED" });
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // DÜZELTME: Port 7185 yapıldı ve yönlendirme Swagger'daki gibi "api/Artist" controller'ına çekildi!
            var response = await client.GetAsync($"https://localhost:7185/api/Artist/GetArtistTopTrack/{artistId}");

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Json(new { success = false, message = "ROLE_INSUFFICIENT" });
            }

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();

                // Dynamic yerine doğrudan kendi DTO'muz ile eşledik, daha güvenli oldu
                var musicData = JsonConvert.DeserializeObject<ResultMusicDto>(jsonData);

                return Json(new { success = true, data = musicData });
            }

            return Json(new { success = false, message = "ERROR" });
        }
    }
}