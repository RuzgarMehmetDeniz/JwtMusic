using JwtMusic.WebUI.Dtos.RolesDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace JwtMusic.WebUI.Controllers
{
    public class AdminRoleController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminRoleController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string? GetToken()
            => HttpContext.Session.GetString("JwtToken")?.Trim().Replace("\"", "");

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = GetToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync("https://localhost:7185/api/Role");

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("SingIn", "Login");

            var roles = new List<ResultRoleDto>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                roles = JsonConvert.DeserializeObject<List<ResultRoleDto>>(json) ?? new();
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Roller alinamadi. ({(int)response.StatusCode}) {errorBody}";
            }

            return View(roles);
        }

        // Create GET
        public IActionResult Create()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            return View();
        }

        // Create POST
        // Not: API 'roleName' parametresini query string ile bekliyor, JSON body ile degil.
        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Rol adi bos olamaz.";
                return View();
            }

            var client = GetClient();
            var url = $"https://localhost:7185/api/Role?roleName={Uri.EscapeDataString(roleName)}";
            var response = await client.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Rol basariyla eklendi.";
                return RedirectToAction("Index");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Rol eklenemedi. ({(int)response.StatusCode}) {errorBody}";
            return View();
        }

        // Update GET
        public async Task<IActionResult> Update(string id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync($"https://localhost:7185/api/Role/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Rol bulunamadi.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var role = JsonConvert.DeserializeObject<ResultRoleDto>(json);

            if (role == null)
            {
                TempData["ErrorMessage"] = "Rol verisi okunamadi.";
                return RedirectToAction("Index");
            }

            return View(role);
        }

        // Update POST
        // Not: API 'roleName' parametresini query string ile bekliyor, JSON body ile degil.
        [HttpPost]
        public async Task<IActionResult> Update(string id, string roleName)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Rol adi bos olamaz.";
                return View(new ResultRoleDto { Id = id, Name = roleName });
            }

            var client = GetClient();
            var url = $"https://localhost:7185/api/Role/{id}?roleName={Uri.EscapeDataString(roleName)}";
            var response = await client.PutAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Rol basariyla guncellendi.";
                return RedirectToAction("Index");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Rol guncellenemedi. ({(int)response.StatusCode}) {errorBody}";
            return View(new ResultRoleDto { Id = id, Name = roleName });
        }

        // Delete POST
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.DeleteAsync($"https://localhost:7185/api/Role/{id}");

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode
                    ? "Rol basariyla silindi."
                    : $"Rol silinemedi. ({(int)response.StatusCode})";

            return RedirectToAction("Index");
        }
    }
}