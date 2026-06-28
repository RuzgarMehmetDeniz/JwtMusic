using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace JwtMusic.WebUI.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RecommendationController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
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
                {
                    { "Basic", 1 }, { "Gold", 2 }, { "Premium", 3 }, { "Elit", 4 }
                };

                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var roles = jwt.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role ||
                                c.Type == "role" || c.Type == "Role")
                    .Select(c => c.Value)
                    .ToList();

                if (!roles.Any()) return "Basic";

                return roles
                    .OrderByDescending(r => roleLevel.TryGetValue(r, out var lvl) ? lvl : 0)
                    .First();
            }
            catch { return "Basic"; }
        }

        // GET /Recommendation/Index
        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            ViewBag.Token = token;
            ViewBag.UserRole = GetRoleFromToken(token);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:7185/api/Recommendation?count=12");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Öneriler yüklenemedi.";
                return View(new List<dynamic>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var songs = JsonConvert.DeserializeObject<List<dynamic>>(json)
                        ?? new List<dynamic>();

            return View(songs);
        }
    }
}
