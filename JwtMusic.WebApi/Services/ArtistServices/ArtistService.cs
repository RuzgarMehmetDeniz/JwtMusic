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
           var values = await _context.Artists.ToListAsync();
            return _mapper.Map<List<ResultArtistDto>>(values);
        }
    }
}
