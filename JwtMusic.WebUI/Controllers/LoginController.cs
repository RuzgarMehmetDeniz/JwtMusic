using JwtMusic.WebUI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
                    return Redirect("/Artist/ArtistList");
                }
            }
            ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
            return View("SingIn"); // Hata durumunda yine senin sayfana döner
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