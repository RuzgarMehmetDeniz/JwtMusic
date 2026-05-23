using JwtMusic.WebUI.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace JwtMusic.WebUI.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult SignUp() // async ihtiyacı olmadığı için kaldırıldı
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterDto registerDto)
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:7185/api/Register", content);

            if (response.IsSuccessStatusCode)
            {
                // Kayıt sonrası senin giriş sayfana yönlendirir
                return RedirectToAction("SignIn", "Login");
            }

            // Kayıt başarısızsa ekranda bir hata göstermek istersen kullanılabilir
            ViewBag.ErrorMessage = "Kayıt işlemi başarısız. Lütfen bilgilerinizi kontrol edin.";
            return View();
        }
    }
}