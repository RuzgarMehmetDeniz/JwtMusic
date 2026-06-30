using JwtMusic.WebUI.Dtos.ArtistDtos;
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
        private const string ApiBaseUrl = "https://localhost:7185/api/Artist";

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

        // Sanatçı listesi (sayfalı)
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync(ApiBaseUrl);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("SingIn", "Login");

            var allArtists = new List<ResultArtistDto>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                allArtists = JsonConvert.DeserializeObject<List<ResultArtistDto>>(json) ?? new List<ResultArtistDto>();
            }
            else
            {
                TempData["ErrorMessage"] = $"Sanatçı listesi alınamadı. ({(int)response.StatusCode})";
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = allArtists.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var pagedArtists = allArtists
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(pagedArtists);
        }

        // Id'ye göre tek sanatçı getirme (detay/güncelleme formu için)
        public async Task<IActionResult> Detail(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync($"{ApiBaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var artist = JsonConvert.DeserializeObject<GetByIdArtistDto>(json);

            return View(artist);
        }

        // Yeni sanatçı ekleme
        [HttpPost]
        public async Task<IActionResult> Create(CreateArtistDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(ApiBaseUrl, content);

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode
                    ? "Sanatçı başarıyla eklendi."
                    : $"Sanatçı eklenemedi. ({(int)response.StatusCode}) {await response.Content.ReadAsStringAsync()}";

            return RedirectToAction("Index");
        }

        // Sanatçı güncelleme
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateArtistDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            dto.ArtistId = id;

            var client = GetClient();
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{ApiBaseUrl}/{id}", content);

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode
                    ? "Sanatçı başarıyla güncellendi."
                    : $"Sanatçı güncellenemedi. ({(int)response.StatusCode}) {await response.Content.ReadAsStringAsync()}";

            return RedirectToAction("Index");
        }

        // Sanatçı silme
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.DeleteAsync($"{ApiBaseUrl}/{id}");

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode
                    ? "Sanatçı başarıyla silindi."
                    : $"Sanatçı silinemedi. ({(int)response.StatusCode}) {await response.Content.ReadAsStringAsync()}";

            return RedirectToAction("Index");
        }
    }
}