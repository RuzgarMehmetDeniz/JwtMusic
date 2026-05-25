using JwtMusic.WebApi.Dtos;

namespace JwtMusic.WebApi.Services.ArtistServices
{
    public interface IArtistService
    {
        Task<List<ResultArtistDto>> GetAllArtistsAsync();

        Task CreateArtistAsync(CreateArtistDto createArtistDto);
        Task<GetArtistWithMusicsDto> GetArtistByIdAsync(int id);
    }
}
