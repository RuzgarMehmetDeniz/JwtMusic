namespace JwtMusic.WebApi.Services.ArtistFollowService
{
    public interface IArtistFollowService
    {
        // true dönerse takip edildi, false dönerse takipten çıkıldı demektir
        Task<bool> ToggleFollowAsync(string userId, int artistId);

        // Sayfa yenilendiğinde butonların durumunu korumak için takip edilen ID listesi
        Task<List<int>> GetFollowedArtistIdsAsync(string userId);
    }
}
