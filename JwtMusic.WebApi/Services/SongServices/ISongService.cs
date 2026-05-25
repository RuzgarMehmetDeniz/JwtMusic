using JwtMusic.WebApi.Dtos;

namespace JwtMusic.WebApi.Services.SongServices
{
    public interface ISongService
    {
        Task<ResultMusicDto> GetArtistTopTrackAsync(int artistId);
    }
}