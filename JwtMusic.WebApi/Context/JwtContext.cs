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
        public DbSet<UserArtistFollow> UserArtistFollows { get; set; }
        public DbSet<ListeningHistory> ListeningHistories { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

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

            modelBuilder.Entity<ListeningHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.AppUser)
                      .WithMany()
                      .HasForeignKey(e => e.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Song)
                      .WithMany()
                      .HasForeignKey(e => e.SongId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ListenedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.HasKey(e => e.PlaylistId);

                entity.HasOne(e => e.AppUser)
                      .WithMany()
                      .HasForeignKey(e => e.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlaylistSong>(entity =>
            {
                entity.HasKey(e => e.PlaylistSongId);

                entity.HasOne(e => e.Playlist)
                      .WithMany(p => p.PlaylistSongs)
                      .HasForeignKey(e => e.PlaylistId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Song)
                      .WithMany()
                      .HasForeignKey(e => e.SongId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}