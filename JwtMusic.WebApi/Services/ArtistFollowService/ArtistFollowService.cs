using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtMusic.WebApi.Services.ArtistFollowService
{
    public class ArtistFollowService : IArtistFollowService
    {
        private readonly JwtContext _context;

        public ArtistFollowService(JwtContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleFollowAsync(string userId, int artistId)
        {
            var existingFollow = await _context.UserArtistFollows
                .FirstOrDefaultAsync(f => f.AppUserId == userId && f.ArtistId == artistId);
            
            if (existingFollow != null)
            {
                // Varsa takipten çık
                _context.UserArtistFollows.Remove(existingFollow);
                await _context.SaveChangesAsync();
                return false;
            }

            // Yoksa takip et
            var newFollow = new UserArtistFollow
            {
                AppUserId = userId,
                ArtistId = artistId,
                FollowedAt = DateTime.UtcNow
            };

            await _context.UserArtistFollows.AddAsync(newFollow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<int>> GetFollowedArtistIdsAsync(string userId)
        {
            return await _context.UserArtistFollows
                .Where(f => f.AppUserId == userId)
                .Select(f => f.ArtistId)
                .ToListAsync();
        }
    }
}
