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

        [HttpGet("GetArtistWithMusics/{id}")]
        [Authorize(Roles = "Premium,Gold,Basic")]
        public async Task<IActionResult> GetArtistWithMusics(int id)
        {
            var values = await _artistService.GetArtistByIdAsync(id);
            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> ArtistList(CreateArtistDto createArtistDto)
        {
            await _artistService.CreateArtistAsync(createArtistDto);
            return Ok("İşlem Başarılı");
        }

        [HttpGet("GetArtistTopTrack/{artistId}")]
        [Authorize(Roles = "Premium,Gold,Basic")]
        public async Task<IActionResult> GetArtistTopTrack(int artistId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
            {
                return Forbid();
            }

            var topTrack = await _songService.GetArtistTopTrackAsync(artistId);

            if (topTrack == null)
            {
                return NotFound("Bu sanatçıya ait şarkı bulunamadı.");
            }

            // Şarkı Premium ise; Basic ve Gold kullanıcılarına engelle, 403 Forbidden dön
            if (topTrack.RequiredRole == "Premium")
            {
                if (userRole == "Basic" || userRole == "Gold")
                {
                    return Forbid();
                }
            }

            return Ok(topTrack);
        }
    }
}