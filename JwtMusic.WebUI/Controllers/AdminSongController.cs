using JwtMusic.WebUI.Dtos.SongDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace JwtMusic.WebUI.Controllers
{
    public class AdminSongController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminSongController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
            => _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = GetToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync("https://localhost:7185/api/Songs");

            var allSongs = new List<ResultSongDto>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                allSongs = JsonConvert.DeserializeObject<List<ResultSongDto>>(json) ?? new();
            }
            else
            {
                TempData["ErrorMessage"] = $"Şarkı listesi alınamadı. ({(int)response.StatusCode})";
            }

            if (page < 1) page = 1;
            var totalCount = allSongs.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var paged = allSongs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(paged);
        }

        public async Task<IActionResult> Create()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            await SetArtistsAndRoles();
            return View(new CreateSongDto { ReleaseDate = DateTime.Today });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSongDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();

            if (!ModelState.IsValid)
            {
                await SetArtistsAndRoles();
                return View(dto);
            }

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7185/api/Songs", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Şarkı başarıyla eklendi.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Şarkı eklenemedi. ({(int)response.StatusCode})";
            await SetArtistsAndRoles();
            return View(dto);
        }
        // ─── Update GET ───────────────────────────────────────────────────────
        public async Task<IActionResult> Update(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();

            // Şarkıyı çek
            var response = await client.GetAsync($"https://localhost:7185/api/Songs/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Şarkı bulunamadı.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var song = JsonConvert.DeserializeObject<ResultSongDto>(json);

            var dto = new UpdateSongDto
            {
                SongId = song.SongId,
                Title = song.Title,
                CoverImageUrl = song.CoverImageUrl,
                AudioUrl = song.AudioUrl,
                Duration = song.Duration.ToString(@"hh\:mm\:ss"),
                ReleaseDate = song.ReleaseDate,
                ArtistId = song.ArtistId,
                RequiredRoleId = song.RequiredRoleId
            };

            await SetArtistsAndRoles();
            return View(dto);
        }

        // ─── Update POST ──────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateSongDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            dto.SongId = id;

            var content = new StringContent(
                JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

            // API: PUT /api/Songs/{id}
            var response = await client.PutAsync($"https://localhost:7185/api/Songs/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Şarkı başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Şarkı güncellenemedi. ({(int)response.StatusCode})";
            await SetArtistsAndRoles();
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.DeleteAsync($"https://localhost:7185/api/Songs/{id}");

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode
                    ? "Şarkı başarıyla silindi."
                    : $"Şarkı silinemedi. ({(int)response.StatusCode})";

            return RedirectToAction("Index");
        }

        private async Task SetArtistsAndRoles()
        {
            var client = GetClient();

            var artistResponse = await client.GetAsync("https://localhost:7185/api/Artist");
            if (artistResponse.IsSuccessStatusCode)
            {
                var json = await artistResponse.Content.ReadAsStringAsync();
                ViewBag.Artists = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
            }
            else ViewBag.Artists = new List<dynamic>();

            var roleResponse = await client.GetAsync("https://localhost:7185/api/Role");
            if (roleResponse.IsSuccessStatusCode)
            {
                var json = await roleResponse.Content.ReadAsStringAsync();
                ViewBag.Roles = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
            }
            else ViewBag.Roles = new List<dynamic>();
        }
    }
}