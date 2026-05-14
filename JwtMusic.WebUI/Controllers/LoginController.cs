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

        public IActionResult SingIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SingIn(LoginDto loginDto)
        {
            var client = _httpClientFactory.CreateClient();
            var JsonData = JsonConvert.SerializeObject(loginDto);
            StringContent stringContent = new StringContent(JsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("https://localhost:7185/api/Login", stringContent);

            var responseJson = await responseMessage.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<ResponseTokenDto>(responseJson);
            string token = tokenResponse.Token;

            HttpContext.Session.SetString("JwtToken", token);
            return View();
        }
    }
}
