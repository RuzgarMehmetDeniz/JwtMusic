using JwtMusic.WebUI.Dtos.AccountDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;


namespace JwtMusic.WebUI.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public AdminDashboardController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("SingIn", "Login");

            var client = GetClient();

            var artists = await GetJsonListAsync(client, "https://localhost:7185/api/Artist");
            var songs = await GetJsonListAsync(client, "https://localhost:7185/api/Songs");
            int roleCount = await GetCountAsync(client, "https://localhost:7185/api/Role");
            var users = await GetUsersAsync(client);

            int artistCount = artists?.Count ?? -1;
            int songCount = songs?.Count ?? -1;
            int userCount = users.Count;

            // Rol dağılımı
            var roleDistribution = new Dictionary<string, int>();
            foreach (var user in users)
            {
                if (user.Roles == null || !user.Roles.Any())
                {
                    roleDistribution.TryGetValue("Rolsuz", out var noRoleCount);
                    roleDistribution["Rolsuz"] = noRoleCount + 1;
                    continue;
                }
                foreach (var role in user.Roles)
                {
                    roleDistribution.TryGetValue(role, out var count);
                    roleDistribution[role] = count + 1;
                }
            }

            // --- Son eklenen sanatçılar ---
            var recentArtists = (artists ?? new List<JObject>())
                .OrderByDescending(a => GetInt(a, "id", "Id", "artistId", "ArtistId"))
                .Take(5)
                .Select(a => new RecentItemViewModel
                {
                    Title = GetString(a, "name", "Name", "artistName", "ArtistName") ?? "İsimsiz",
                    SubTitle = null
                }).ToList();

            // --- Son eklenen şarkılar ---
            var recentSongs = (songs ?? new List<JObject>())
                .OrderByDescending(s => GetInt(s, "id", "Id", "songId", "SongId"))
                .Take(5)
                .Select(s => new RecentItemViewModel
                {
                    Title = GetString(s, "name", "Name", "songName", "SongName", "title", "Title") ?? "İsimsiz",
                    SubTitle = GetString(s, "artistName", "ArtistName")
                }).ToList();

            // --- Son kayıt olan kullanıcılar ---
            var recentUsers = users
                .OrderByDescending(u => u.Id)
                .Take(5)
                .Select(u => new RecentItemViewModel
                {
                    Title = u.UserName ?? u.Email ?? "İsimsiz",
                    SubTitle = u.Roles != null && u.Roles.Any() ? string.Join(", ", u.Roles) : "Rolsuz"
                }).ToList();

            // --- En çok şarkısı olan sanatçılar ---
            var topArtists = (songs ?? new List<JObject>())
                .Select(s => GetString(s, "artistName", "ArtistName"))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .GroupBy(name => name!)
                .Select(g => new TopArtistViewModel { ArtistName = g.Key, SongCount = g.Count() })
                .OrderByDescending(g => g.SongCount)
                .Take(5)
                .ToList();
            int maxTopArtistCount = topArtists.Any() ? topArtists.Max(a => a.SongCount) : 1;

            ViewBag.ArtistCount = artistCount;
            ViewBag.SongCount = songCount;
            ViewBag.RoleCount = roleCount;
            ViewBag.UserCount = userCount;
            ViewBag.RoleDistribution = roleDistribution;
            ViewBag.RecentArtists = recentArtists;
            ViewBag.RecentSongs = recentSongs;
            ViewBag.RecentUsers = recentUsers;
            ViewBag.TopArtists = topArtists;
            ViewBag.MaxTopArtistCount = maxTopArtistCount;

            return View();
        }

        // JObject içinden, verilen aday alan adlarından ilk bulunanı int olarak döndürür. Bulamazsa 0.
        private static int GetInt(JObject obj, params string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                var token = obj[name];
                if (token != null && token.Type != JTokenType.Null)
                {
                    if (int.TryParse(token.ToString(), out var value))
                        return value;
                }
            }
            return 0;
        }

        // JObject içinden, verilen aday alan adlarından ilk bulunanı string olarak döndürür. Bulamazsa null.
        private static string? GetString(JObject obj, params string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                var token = obj[name];
                if (token != null && token.Type != JTokenType.Null)
                {
                    var value = token.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }
            return null;
        }

        private static async Task<int> GetCountAsync(HttpClient client, string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return -1;
                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<object>>(json);
                return list?.Count ?? 0;
            }
            catch
            {
                return -1;
            }
        }

        // Ham JSON listesini JObject olarak döndürür (case-insensitive alan erişimi için).
        private static async Task<List<JObject>?> GetJsonListAsync(HttpClient client, string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JObject>>(json);
            }
            catch
            {
                return null;
            }
        }

        private static async Task<List<ResultAccountDto>> GetUsersAsync(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("https://localhost:7185/api/User");
                if (!response.IsSuccessStatusCode)
                    return new List<ResultAccountDto>();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ResultAccountDto>>(json) ?? new List<ResultAccountDto>();
            }
            catch
            {
                return new List<ResultAccountDto>();
            }
        }
    }

    public class RecentItemViewModel
    {
        public string Title { get; set; } = "";
        public string? SubTitle { get; set; }
    }

    public class TopArtistViewModel
    {
        public string ArtistName { get; set; } = "";
        public int SongCount { get; set; }
    }
}