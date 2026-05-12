using JwtMusic.WebApi.Dtos;
using JwtMusic.WebApi.Services.ArtistServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly IArtistService _artistService; 

        public ArtistController(IArtistService artistService)
        {
            _artistService = artistService;
        }
        [HttpGet]
        public async Task<IActionResult> ArtistList()
        {
            var values = await _artistService.GetAllArtistsAsync();
            return Ok(values);
        }
        [HttpPost]
        public async Task<IActionResult> ArtistList(CreateArtistDto createArtistDto)
        {
            await _artistService.CreateArtistAsync(createArtistDto);
            return Ok("İşlem Başarılı");
        }
    }
}
