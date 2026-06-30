using AutoMapper;
using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Dtos.ArtistDtos;
using JwtMusic.WebApi.Entities;
using Microsoft.EntityFrameworkCore;
namespace JwtMusic.WebApi.Services.ArtistServices
{
    public class ArtistService : IArtistService
    {
        private readonly JwtContext _context;
        private readonly IMapper _mapper;
        public ArtistService(JwtContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task CreateArtistAsync(CreateArtistDto createArtistDto)
        {
            var value = _mapper.Map<Artist>(createArtistDto);
            await _context.Artists.AddAsync(value);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ResultArtistDto>> GetAllArtistsAsync()
        {
            var values = await _context.Artists
                .Join(_context.Roles,
                    artist => artist.RequiredRole,
                    role => role.Id,
                    (artist, role) => new ResultArtistDto
                    {
                        ArtistId = artist.ArtistId,
                        Name = artist.Name,
                        ImageUrl = artist.ImageUrl,
                        Bio = artist.Bio,
                        MonthlyListeners = artist.MonthlyListeners,
                        IsVerified = artist.IsVerified,
                        RequiredRoleId = artist.RequiredRole,
                        RequiredRoleName = role.Name
                    })
                .ToListAsync();
            return values;
        }

        public async Task<ResultArtistDto> GetByIdArtistAsync(int id)
        {
            var value = await _context.Artists
                .Where(x => x.ArtistId == id)
                .Join(_context.Roles,
                    artist => artist.RequiredRole,
                    role => role.Id,
                    (artist, role) => new ResultArtistDto
                    {
                        ArtistId = artist.ArtistId,
                        Name = artist.Name,
                        ImageUrl = artist.ImageUrl,
                        Bio = artist.Bio,
                        MonthlyListeners = artist.MonthlyListeners,
                        IsVerified = artist.IsVerified,
                        RequiredRoleId = artist.RequiredRole,
                        RequiredRoleName = role.Name
                    })
                .FirstOrDefaultAsync();
            return value;
        }

        // YENİ: Sanatçı Güncelleme
        public async Task UpdateArtistAsync(UpdateArtistDto updateArtistDto)
        {
            var artist = await _context.Artists.FindAsync(updateArtistDto.ArtistId);
            if (artist == null)
                throw new KeyNotFoundException("Sanatçı bulunamadı.");

            artist.Name = updateArtistDto.Name;
            artist.Bio = updateArtistDto.Bio;
            artist.ImageUrl = updateArtistDto.ImageUrl;
            artist.MonthlyListeners = updateArtistDto.MonthlyListeners;
            artist.IsVerified = updateArtistDto.IsVerified;
            artist.RequiredRole = updateArtistDto.RequiredRole;

            await _context.SaveChangesAsync();
        }

        // YENİ: Sanatçı Silme
        public async Task DeleteArtistAsync(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
                throw new KeyNotFoundException("Sanatçı bulunamadı.");

            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
        }
    }
}