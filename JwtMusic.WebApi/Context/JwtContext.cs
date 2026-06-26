using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtMusic.WebApi.Context
{
    public class JwtContext : IdentityDbContext<AppUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=NıTRO-AN515-57;Database=JwtMusicDb;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;"
            );
        }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Song> Songs { get; set; }
        // 1. Yeni takip ara tablomuzu DbSet olarak ekliyoruz
        public DbSet<UserArtistFollow> UserArtistFollows { get; set; }

        // 2. İlişki kurallarını buraya yazıyoruz
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity tablolarının kurallarını ezmemek için bu base çağrısı ŞART
            base.OnModelCreating(modelBuilder);

            // İki ID'yi birleştirerek birleşik anahtar (Composite Key) yapıyoruz
            modelBuilder.Entity<UserArtistFollow>()
                .HasKey(f => new { f.AppUserId, f.ArtistId });

            // Kullanıcı ile olan ilişki
            modelBuilder.Entity<UserArtistFollow>()
                .HasOne(f => f.AppUser)
                .WithMany(u => u.UserArtistFollows)
                .HasForeignKey(f => f.AppUserId);

            // Sanatçı ile olan ilişki
            modelBuilder.Entity<UserArtistFollow>()
                .HasOne(f => f.Artist)
                .WithMany(a => a.UserArtistFollows)
                .HasForeignKey(f => f.ArtistId);
        }
    }
}