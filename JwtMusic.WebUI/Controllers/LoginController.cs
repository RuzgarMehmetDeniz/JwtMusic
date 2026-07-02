using JwtMusic.WebUI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtMusic.WebUI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult SingIn()
        {
            // Dosya adın SingIn olduğu için view adını açıkça belirttik
            return View("SingIn");
        }

        [HttpPost]
        public async Task<IActionResult> SingIn(LoginDto loginDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(loginDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("https://localhost:7185/api/Login", stringContent);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<ResponseTokenDto>(responseJson);
                string token = tokenResponse?.Token;

                if (!string.IsNullOrEmpty(token))
                {
                    HttpContext.Session.SetString("JwtToken", token);

                    // Token içindeki rollere bakıp Admin ise dashboard'a yönlendir
                    if (TokenHasAdminRole(token))
                    {
                        return Redirect("/AdminDashboard/Index");
                    }

                    return Redirect("/Artist/ArtistList");
                }
            }

            ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
            return View("SingIn"); // Hata durumunda yine senin sayfana döner
        }

        // JWT içindeki rol claim'lerini okuyup "Admin" var mı diye kontrol eder.
        // Token burada sadece okunuyor (decode), imza doğrulaması yapılmıyor —
        // çünkü token zaten API tarafından üretilip güvenilir şekilde geldi.
        private static bool TokenHasAdminRole(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                    return false;

                var jwt = handler.ReadJwtToken(token);

                // Roller genelde "role" veya ClaimTypes.Role ("http://schemas.../role") olarak gelir,
                // API'nize göre ikisini de kontrol ediyoruz.
                var roleClaims = jwt.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value);

                return roleClaims.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            HttpContext.Session.Clear();
            return Redirect("/Login/SingIn");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}