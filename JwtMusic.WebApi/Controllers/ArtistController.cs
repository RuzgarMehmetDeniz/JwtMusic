using JwtMusic.WebApi.Dtos;
using JwtMusic.WebApi.Services.ArtistServices;
using JwtMusic.WebApi.Services.SongServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly IArtistService _artistService;
        private readonly ISongService _songService;

        public ArtistController(IArtistService artistService, ISongService songService)
        {
            _artistService = artistService;
            _songService = songService;
        }

        [HttpGet]
        [Authorize(Roles = "Premium,Gold,Basic")]
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