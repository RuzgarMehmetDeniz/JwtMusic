using JwtMusic.WebUI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JwtMusic.WebUI.Controllers
{
    public class ArtistController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ArtistController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> ArtistList()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7185/api/Artist");
            var jsonData = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<ResultArtistDto>>(jsonData);
            return View(values);
        }
    }
}
