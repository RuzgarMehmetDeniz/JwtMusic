using JwtMusic.WebApi.Dtos.AccountDtos;
using JwtMusic.WebUI.Dtos.AccountDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ResultAccountDto = JwtMusic.WebUI.Dtos.AccountDtos.ResultAccountDto;
using UpdateAccountDto = JwtMusic.WebUI.Dtos.AccountDtos.UpdateAccountDto;

namespace JwtMusic.WebUI.Controllers
{
    public class AdminUserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminUserController(IHttpClientFactory httpClientFactory)
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

        // Roller (checkbox listesi icin) - api/Role'den isim bazli okunuyor
        private async Task<List<string>> GetAllRoleNamesAsync()
        {
            var roleNames = new List<string>();

            try
            {
                var client = GetClient();
                var response = await client.GetAsync("https://localhost:7185/api/Role");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jArray = JsonConvert.DeserializeObject<List<JObject>>(json) ?? new List<JObject>();

                    foreach (var item in jArray)
                    {
                        var name = item.GetValue("Name", StringComparison.OrdinalIgnoreCase)?.ToString();
                        if (!string.IsNullOrEmpty(name))
                            roleNames.Add(name);
                    }
                }
            }
            catch
            {
                // Roller alinamazsa bos liste ile devam edilir.
            }

            return roleNames;
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync("https://localhost:7185/api/User");

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("SingIn", "Login");

            var users = new List<ResultAccountDto>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                users = JsonConvert.DeserializeObject<List<ResultAccountDto>>(json) ?? new();
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Kullanicilar alinamadi. ({(int)response.StatusCode}) {errorBody}";
            }

            return View(users);
        }

        // Create GET
        public IActionResult Create()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            return View(new CreateAccountDto());
        }

        // Create POST - mevcut api/Register endpoint'i kullanilir
        [HttpPost]
        public async Task<IActionResult> Create(CreateAccountDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7185/api/Register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Kullanici basariyla eklendi.";
                return RedirectToAction("Index");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Kullanici eklenemedi. ({(int)response.StatusCode}) {errorBody}";
            return View(dto);
        }

        // Update GET
        public async Task<IActionResult> Update(string id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.GetAsync($"https://localhost:7185/api/User/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Kullanici bulunamadi.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<ResultAccountDto>(json);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanici verisi okunamadi.";
                return RedirectToAction("Index");
            }

            var dto = new UpdateAccountDto
            {
                Id = user.Id,
                Name = user.FirstName,
                Surname = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                Roles = user.Roles
            };

            ViewBag.AllRoles = await GetAllRoleNamesAsync();

            return View(dto);
        }

        // Update POST
        [HttpPost]
        public async Task<IActionResult> Update(string id, UpdateAccountDto dto, List<string>? Roles)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            dto.Id = id;
            dto.Roles = Roles ?? new List<string>();

            var client = GetClient();
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"https://localhost:7185/api/User/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Kullanici basariyla guncellendi.";
                return RedirectToAction("Index");
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Kullanici guncellenemedi. ({(int)response.StatusCode}) {errorBody}";

            ViewBag.AllRoles = await GetAllRoleNamesAsync();
            return View(dto);
        }

        // Delete POST
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();
            var response = await client.DeleteAsync($"https://localhost:7185/api/User/{id}");

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode
                    ? "Kullanici basariyla silindi."
                    : $"Kullanici silinemedi. ({(int)response.StatusCode})";

            return RedirectToAction("Index");
        }
    }
}