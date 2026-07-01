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
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Login"); // Giriş sayfasına güvenli yönlendirme

            var client = GetClient();
            // KRİTİK HATA DÜZELTMESİ: Veriyi çekerken de API [Authorize] olduğu için token eklemek zorundayız!
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // API adresi çağrılırken BaseAddress çakışmasını önlemek için absolute URL garantileniyor
            var response = await client.GetAsync($"https://localhost:7185/api/Songs/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = $"Şarkı bilgileri getirilemedi. API Kodu: {(int)response.StatusCode}";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var song = JsonConvert.DeserializeObject<ResultSongDto>(json);

            if (song == null)
            {
                TempData["ErrorMessage"] = "Şarkı verisi çözümlenemedi.";
                return RedirectToAction("Index");
            }

            var dto = new UpdateSongDto
            {
                SongId = song.SongId,
                Title = song.Title,
                CoverImageUrl = song.CoverImageUrl,
                AudioUrl = song.AudioUrl,
                Duration = song.Duration.ToString(), // API'den TimeSpan geliyorsa ToString() yeterlidir
                ReleaseDate = song.ReleaseDate,
                ArtistId = song.ArtistId,
                RequiredRoleId = song.RequiredRoleId
            };

            await SetArtistsAndRoles();
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateSongDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Login");

            var client = GetClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if (!ModelState.IsValid)
            {
                await SetArtistsAndRoles();
                return View(dto);
            }

            // Güvenlik ve eşleşme için id'yi dto'ya dolduruyoruz
            dto.SongId = id;
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

            // CRITICAL: Tam URL yazmak yerine sadece "api/Songs/{id}" yazıyoruz!
            // GetClient() zaten localhost:7185 kısmını otomatik ekliyor.
            var response = await client.PutAsync($"https://localhost:7185/api/Songs/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Şarkı başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            // Eğer yine hata alırsan hatanın kodunu görebilmek için:
            TempData["ErrorMessage"] = $"Şarkı güncellenemedi. API Kodu: {(int)response.StatusCode}";
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