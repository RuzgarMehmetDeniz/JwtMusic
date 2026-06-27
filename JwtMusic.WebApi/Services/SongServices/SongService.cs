using JwtMusic.WebApi.Dtos;
using JwtMusic.WebApi.Context; // Kendi DbContext namespace'ini kontrol etmeyi unutma
using Microsoft.EntityFrameworkCore;

namespace JwtMusic.WebApi.Services.SongServices
{
    public class SongService : ISongService
    {
        private readonly JwtContext _context;

        public SongService(JwtContext context)
        {
            _context = context;
        }

        public async Task<ResultMusicDto> GetArtistTopTrackAsync(int artistId)
        {
            // Veri tabanından en yüksek dinlenmeye (PlayCount) sahip şarkıyı tamamen dinamik getiriyoruz
            var song = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.ArtistId == artistId)
                .OrderByDescending(s => s.PlayCount)
                .Select(s => new ResultMusicDto
                {
                    MusicId = s.SongId,
                    Title = s.Title,
                    FilePath = s.AudioUrl,

                    RequiredRole = s.RequiredRole ,

                    ArtistId = s.ArtistId,
                    ArtistName = s.Artist.Name
                })
                .FirstOrDefaultAsync();

            return song;
        }
    }
}