using AutoMapper;
using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Dtos;
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

        // DOĞRU VE GÜNCEL METOT
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
    }
}