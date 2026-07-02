using JwtMusic.WebUI.Dtos.ArtistDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            var response = await client.GetAsync("https://localhost:7185/api/Artist");

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("SingIn", "Login");

            var allArtists = new List<ResultArtistDto>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                allArtists = JsonConvert.DeserializeObject<List<ResultArtistDto>>(json) ?? new();
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Sanatci listesi alinamadi. ({(int)response.StatusCode}) {errorBody}";
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = allArtists.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var paged = allArtists.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

            await SetRolesAsync();

            return View(new CreateArtistDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateArtistDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            if (!ModelState.IsValid)
            {
                await SetRolesAsync();
                return View(dto);
            }

            var client = GetClient();
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7185/api/Artist", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Sanatci basariyla eklendi.";
                return RedirectToAction("Index");
            }

            await SetRolesAsync();
            var errorBody = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Sanatci eklenemedi. ({(int)response.StatusCode}) {errorBody}";
            return View(dto);
        }

        public async Task<IActionResult> Update(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            await SetRolesAsync();

            var client = GetClient();
            var response = await client.GetAsync($"https://localhost:7185/api/Artist/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Sanatci bulunamadi.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var artist = JsonConvert.DeserializeObject<GetByIdArtistDto>(json);

            if (artist == null)
            {
                TempData["ErrorMessage"] = "Sanatci verisi okunamadi.";
                return RedirectToAction("Index");
            }

            var dto = new UpdateArtistDto
            {
                ArtistId = artist.ArtistId,
                Name = artist.Name,
                Bio = artist.Bio,
                ImageUrl = artist.ImageUrl,
                MonthlyListeners = artist.MonthlyListeners,
                IsVerified = artist.IsVerified,
                RequiredRole = artist.RequiredRole
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateArtistDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            dto.ArtistId = id;

            ModelState.Remove(nameof(dto.ArtistId));
            if (!TryValidateModel(dto))
            {
                await SetRolesAsync();
                return View(dto);
            }

            var client = GetClient();
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"https://localhost:7185/api/Artist/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Sanatci basariyla guncellendi.";
                return RedirectToAction("Index");
            }

            await SetRolesAsync();
            var errorBody = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Sanatci guncellenemedi. ({(int)response.StatusCode}) {errorBody}";
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
                    ? "Sanatci basariyla silindi."
                    : $"Sanatci silinemedi. ({(int)response.StatusCode})";

            return RedirectToAction("Index");
        }
        private async Task SetRolesAsync()
        {
            var client = GetClient();
            var roles = new List<Dictionary<string, string>>();

            try
            {
                var roleResponse = await client.GetAsync("https://localhost:7185/api/Role");

                if (roleResponse.IsSuccessStatusCode)
                {
                    var json = await roleResponse.Content.ReadAsStringAsync();
                    var jArray = JsonConvert.DeserializeObject<List<JObject>>(json) ?? new List<JObject>();

                    foreach (var item in jArray)
                    {
                        var id = GetFirstValue(item, "RoleId", "Id", "roleId", "id", "ID", "RoleID");
                        var name = GetFirstValue(item, "RoleName", "Name", "roleName", "name", "Role", "Title", "RoleTitle");

                        if (string.IsNullOrEmpty(id))
                            continue;

                        roles.Add(new Dictionary<string, string>
                        {
                            ["Id"] = id,
                            ["Name"] = string.IsNullOrEmpty(name) ? id : name
                        });
                    }
                }
            }
            catch
            {
                // Roller alinamadiysa bos liste ile devam edilir, view null-safe calisir.
            }

            ViewBag.Roles = roles;
        }

        private static string? GetFirstValue(JObject obj, params string[] keys)
        {
            foreach (var key in keys)
            {
                var token = obj.GetValue(key, StringComparison.OrdinalIgnoreCase);
                if (token != null && token.Type != JTokenType.Null)
                    return token.ToString();
            }
            return null;
        }
    }
}