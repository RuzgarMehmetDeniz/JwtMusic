using JwtMusic.WebApi.Dtos.ArtistDtos;
using JwtMusic.WebApi.Services.ArtistServices;
using JwtMusic.WebApi.Services.SongServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Premium,Gold,Basic")] // Tüm metotları koruma altına alıyoruz
    public class ArtistController : ControllerBase
    {
        private readonly IArtistService _artistService;
        private readonly ISongService _songService;

        public ArtistController(IArtistService artistService, ISongService songService)
        {
            _artistService = artistService;
            _songService = songService;
        }

        // 1. Tüm Sanatçıları Listeleme
        [HttpGet]
        public async Task<IActionResult> ArtistList()
        {
            var values = await _artistService.GetAllArtistsAsync();
            return Ok(values);
        }

        // 2. ID'ye Göre Tekil Sanatçı Getirme (Detay Sayfası İçin)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArtistById(int id)
        {
            var value = await _artistService.GetByIdArtistAsync(id);
            if (value == null)
            {
                return NotFound("Sanatçı bulunamadı.");
            }
            return Ok(value);
        }

        // 3. Yeni Sanatçı Ekleme
        [HttpPost]
        public async Task<IActionResult> CreateArtist(CreateArtistDto createArtistDto)
        {
            // Validasyondan geçemezse buraya düşer ve hataları haber verir
            if (!ModelState.IsValid)
            {
                // FluentValidation'ın ürettiği tüm hata mesajlarını listeye topluyoruz
                var errorList = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                // Dış dünyaya (UI veya Postman'e) hataları dönüyoruz
                return BadRequest(new
                {
                    IsSuccess = false,
                    Message = "Validasyon hataları oluştu.",
                    Errors = errorList
                });
            }

            await _artistService.CreateArtistAsync(createArtistDto);
            return Ok("İşlem Başarılı");
        }

        // 4. Sanatçı Güncelleme
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArtist(int id, UpdateArtistDto updateArtistDto)
        {
            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    IsSuccess = false,
                    Message = "Validasyon hataları oluştu.",
                    Errors = errorList
                });
            }

            var existing = await _artistService.GetByIdArtistAsync(id);
            if (existing == null)
            {
                return NotFound("Sanatçı bulunamadı.");
            }
            updateArtistDto.ArtistId = id;
            await _artistService.UpdateArtistAsync(updateArtistDto);
            return Ok("Sanatçı güncellendi.");
        }

        // 5. Sanatçı Silme
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtist(int id)
        {
            var existing = await _artistService.GetByIdArtistAsync(id);
            if (existing == null)
            {
                return NotFound("Sanatçı bulunamadı.");
            }
            await _artistService.DeleteArtistAsync(id);
            return Ok("Sanatçı silindi.");
        }
    }
}