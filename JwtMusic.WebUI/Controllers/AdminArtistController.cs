using JwtMusic.WebUI.Dtos.ArtistDtos;
using JwtMusic.WebUI.Dtos.RolesDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace JwtMusic.WebUI.Controllers
{
    public class AdminArtistController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminArtistController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string? GetToken()
            => HttpContext.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = GetToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // Rolleri API'den çekmek için ortak metod
        private async Task<List<RoleDto>> GetRolesAsync(HttpClient client)
        {
            var response = await client.GetAsync("https://localhost:7185/api/Role");
            if (!response.IsSuccessStatusCode)
                return new List<RoleDto>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<RoleDto>>(json) ?? new List<RoleDto>();
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();

            var listResponse = await client.GetAsync("https://localhost:7185/api/Artist");

            if (listResponse.StatusCode == HttpStatusCode.Unauthorized || listResponse.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("SingIn", "Login");

            var allArtistsSummary = new List<ResultArtistDto>();

            if (listResponse.IsSuccessStatusCode)
            {
                var json = await listResponse.Content.ReadAsStringAsync();
                allArtistsSummary = JsonConvert.DeserializeObject<List<ResultArtistDto>>(json) ?? new List<ResultArtistDto>();
            }
            else
            {
                TempData["ErrorMessage"] = $"Sanatçı listesi alınamadı. ({(int)listResponse.StatusCode})";
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = 0;
                ViewBag.TotalPages = 0;
                return View(new List<GetByIdArtistDto>());
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = allArtistsSummary.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var pagedIds = allArtistsSummary
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => a.ArtistId)
                .ToList();

            var pagedArtists = new List<GetByIdArtistDto>();
            foreach (var id in pagedIds)
            {
                var detailResponse = await client.GetAsync($"https://localhost:7185/api/Artist/{id}");

                if (detailResponse.IsSuccessStatusCode)
                {
                    var detailJson = await detailResponse.Content.ReadAsStringAsync();
                    var artist = JsonConvert.DeserializeObject<GetByIdArtistDto>(detailJson);
                    if (artist != null)
                        pagedArtists.Add(artist);
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(pagedArtists);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync($"https://localhost:7185/api/Artist/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var artist = JsonConvert.DeserializeObject<GetByIdArtistDto>(json);

            return View(artist);
        }

        // Yeni sanatçı ekleme formu (GET)
        public async Task<IActionResult> Create()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            ViewBag.Roles = await GetRolesAsync(client);

            return View(new CreateArtistDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateArtistDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await GetRolesAsync(client);
                return View(dto);
            }

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7185/api/Artist", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Sanatçı başarıyla eklendi.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Sanatçı eklenemedi. ({(int)response.StatusCode}) {await response.Content.ReadAsStringAsync()}";
            ViewBag.Roles = await GetRolesAsync(client);
            return View(dto);
        }

        // Güncelleme formu (GET)
        public async Task<IActionResult> Update(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync($"https://localhost:7185/api/Artist/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Sanatçı bulunamadı.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var artist = JsonConvert.DeserializeObject<GetByIdArtistDto>(json);

            var updateDto = new UpdateArtistDto
            {
                ArtistId = artist.ArtistId,
                Name = artist.Name,
                Bio = artist.Bio,
                ImageUrl = artist.ImageUrl,
                MonthlyListeners = artist.MonthlyListeners,
                IsVerified = artist.IsVerified,
                RequiredRole = artist.RequiredRole
            };

            ViewBag.Roles = await GetRolesAsync(client);

            return View(updateDto);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateArtistDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await GetRolesAsync(client);
                return View(dto);
            }

            dto.ArtistId = id;

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"https://localhost:7185/api/Artist/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Sanatçı başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Sanatçı güncellenemedi. ({(int)response.StatusCode}) {await response.Content.ReadAsStringAsync()}";
            ViewBag.Roles = await GetRolesAsync(client);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.DeleteAsync($"https://localhost:7185/api/Artist/{id}");

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode
                    ? "Sanatçı başarıyla silindi."
                    : $"Sanatçı silinemedi. ({(int)response.StatusCode}) {await response.Content.ReadAsStringAsync()}";

            return RedirectToAction("Index");
        }
    }
}