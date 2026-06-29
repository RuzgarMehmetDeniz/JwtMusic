using JwtMusic.WebUI.Dtos.PlaylistDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;

namespace JwtMusic.WebUI.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string API_BASE = "https://localhost:7185";

        public PlaylistController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
            => _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                                               ?.Trim().Replace("\"", "");

        private string GetRoleFromToken(string token)
        {
            try
            {
                var roleLevel = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                    { { "Basic", 1 }, { "Gold", 2 }, { "Premium", 3 }, { "Elit", 4 } };

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var roles = jwt.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role ||
                                c.Type == "role" || c.Type == "Role")
                    .Select(c => c.Value).ToList();

                if (!roles.Any()) return "Basic";

                return roles.OrderByDescending(r => roleLevel.TryGetValue(r, out var l) ? l : 0).First();
            }
            catch { return "Basic"; }
        }

        private HttpClient CreateAuthClient(string token)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ── GET /Playlist/Index — Tüm listeler ──────────────────────────
        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            ViewBag.Token = token;
            ViewBag.UserRole = GetRoleFromToken(token);

            var client = CreateAuthClient(token);
            var response = await client.GetAsync($"{API_BASE}/api/Playlist");

            if (!response.IsSuccessStatusCode)
                return View(new List<ResultPlaylistDto>());

            var json = await response.Content.ReadAsStringAsync();
            var playlists = JsonConvert.DeserializeObject<List<ResultPlaylistDto>>(json)
                            ?? new List<ResultPlaylistDto>();

            return View(playlists);
        }

        // ── GET /Playlist/Detail/{id} — Liste detayı ────────────────────
        public async Task<IActionResult> Detail(int id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            ViewBag.Token = token;
            ViewBag.UserRole = GetRoleFromToken(token);

            var client = CreateAuthClient(token);
            var response = await client.GetAsync($"{API_BASE}/api/Playlist/{id}");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var detail = JsonConvert.DeserializeObject<PlaylistDetailDto>(json);

            return View(detail);
        }
    }
}