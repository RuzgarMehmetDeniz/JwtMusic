using JwtMusic.WebUI.Dtos.SongDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt; 

namespace JwtMusic.WebUI.Controllers
{
    public class SongController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SongController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
            => _httpContextAccessor.HttpContext?.Session.GetString("JwtToken")
                                               ?.Trim()
                                               .Replace("\"", "");

        private static readonly Dictionary<string, int> RoleLevel = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Basic",   1 },
            { "Gold",    2 },
            { "Premium", 3 }
        };

        private string GetRoleFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var roles = jwt.Claims
                    .Where(c =>
                        c.Type == System.Security.Claims.ClaimTypes.Role ||
                        c.Type == "role" ||
                        c.Type == "Role")
                    .Select(c => c.Value)
                    .ToList();

                if (roles.Count == 0) return "Basic";

                return roles
                    .OrderByDescending(r => RoleLevel.TryGetValue(r, out var lvl) ? lvl : 0)
                    .First();
            }
            catch
            {
                return "Basic";
            }
        }

        public async Task<IActionResult> SongList()
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            ViewBag.UserRole = GetRoleFromToken(token);
            ViewBag.Token = token;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:7185/api/Songs");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Login");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("SingIn", "Login");

            var json = await response.Content.ReadAsStringAsync();
            var songs = JsonConvert.DeserializeObject<List<ResultSongDto>>(json)
                        ?? new List<ResultSongDto>();

            return View(songs);
        }
    }
}