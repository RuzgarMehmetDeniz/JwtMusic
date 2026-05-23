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
        public IActionResult SignIn()
        {
            // Dosya adın SingIn olduğu için view adını açıkça belirttik
            return View("SingIn");
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginDto loginDto)
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
                    return RedirectToAction("ArtistList", "Artist");
                }
            }

            ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
            return View("SingIn"); // Hata durumunda yine senin sayfana döner
        }

        public IActionResult AccessDenied()
        {
            return Content("Bu sayfayı görmek için Premium üye olmalısınız.");
        }
    }
}