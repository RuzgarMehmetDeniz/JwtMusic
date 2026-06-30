using JwtMusic.WebUI.Dtos.AccountDtos;   // ✅ böyle olmalı
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace JwtMusic.WebUI.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public SettingsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("/Login/SingIn");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://localhost:7185/api/Account/me");
            if (!response.IsSuccessStatusCode)
            {
                return Redirect("/Login/SingIn");
            }

            var json = await response.Content.ReadAsStringAsync();
            var accountInfo = JsonConvert.DeserializeObject<ResultAccountDto>(json);

            ViewBag.Token = token;
            return View(accountInfo);
        }
    }
}