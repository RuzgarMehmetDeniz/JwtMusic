using JwtMusic.WebApi.Dtos.ArtistDtos;

namespace JwtMusic.WebApi.Services.ArtistServices
{
    public interface IArtistService
    {
        Task<List<ResultArtistDto>> GetAllArtistsAsync();
        Task CreateArtistAsync(CreateArtistDto createArtistDto);
        Task<ResultArtistDto> GetByIdArtistAsync(int id);
        Task UpdateArtistAsync(UpdateArtistDto updateArtistDto);
        Task DeleteArtistAsync(int id);
    }
}