using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Dtos.ML;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace JwtMusic.WebApi.Services.MLServices
{
    public class RecommendationService
    {
        private readonly JwtContext _context;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(JwtContext context, ILogger<RecommendationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<int>> GetRecommendedSongIdsAsync(string userId, int topN = 10)
        {
            try
            {
                // 1) Tüm dinleme geçmişini çek
                var allHistory = await _context.ListeningHistories
                    .Include(h => h.Song)
                    .ToListAsync();

                if (allHistory.Count < 5)
                {
                    _logger.LogWarning("Öneri için yeterli veri yok. Kayıt sayısı: {Count}", allHistory.Count);
                    return await GetFallbackRecommendations(userId, topN);
                }

                // 2) Kullanıcı ID'lerini int'e map'le
                //    ML.NET matrix factorization float ister
                var allUserIds = allHistory.Select(h => h.AppUserId).Distinct().ToList();
                var userIdMap = allUserIds.Select((id, idx) => new { id, idx })
                                           .ToDictionary(x => x.id, x => (float)(x.idx + 1));

                // Mevcut kullanıcı map'te yoksa fallback
                if (!userIdMap.ContainsKey(userId))
                    return await GetFallbackRecommendations(userId, topN);

                // 3) Kullanıcı-şarkı etkileşim skoru hesapla
                //    Aynı şarkıyı birden fazla dinlemişse skoru artar (max 5)
                var ratingData = allHistory
                    .GroupBy(h => new { h.AppUserId, h.SongId })
                    .Select(g => new SongRating
                    {
                        UserId = userIdMap[g.Key.AppUserId],
                        SongId = (float)g.Key.SongId,
                        Label = Math.Min(5f, g.Count())  // max 5
                    })
                    .ToList();

                // 4) ML.NET context ve pipeline oluştur
                var mlContext = new MLContext(seed: 42);

                var dataView = mlContext.Data.LoadFromEnumerable(ratingData);

                var options = new MatrixFactorizationTrainer.Options
                {
                    MatrixColumnIndexColumnName = nameof(SongRating.UserId),
                    MatrixRowIndexColumnName = nameof(SongRating.SongId),
                    LabelColumnName = nameof(SongRating.Label),
                    NumberOfIterations = 20,
                    ApproximationRank = 8,   // Az veriyle küçük rank daha iyi
                    LearningRate = 0.4,
                    Lambda = 0.01
                };

                // Key tipine dönüştür (ML.NET gereksinimi)
                var pipeline = mlContext.Transforms
                    .Conversion.MapValueToKey(
                        inputColumnName: nameof(SongRating.UserId),
                        outputColumnName: nameof(SongRating.UserId))
                    .Append(mlContext.Transforms.Conversion.MapValueToKey(
                        inputColumnName: nameof(SongRating.SongId),
                        outputColumnName: nameof(SongRating.SongId)))
                    .Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));

                // 5) Modeli eğit
                var model = pipeline.Fit(dataView);

                // 6) Kullanıcının hiç dinlemediği şarkıları bul
                var userListenedSongIds = allHistory
                    .Where(h => h.AppUserId == userId)
                    .Select(h => h.SongId)
                    .Distinct()
                    .ToHashSet();

                var allSongIds = await _context.Songs
                    .Select(s => s.SongId)
                    .ToListAsync();

                var unheardSongIds = allSongIds
                    .Where(id => !userListenedSongIds.Contains(id))
                    .ToList();

                if (!unheardSongIds.Any())
                {
                    // Hepsini dinlemişse, en yüksek skorluları tekrar öner
                    unheardSongIds = allSongIds;
                }

                // 7) Dinlemediği her şarkı için skor tahmin et
                var currentUserFloat = userIdMap[userId];
                var predEngine = mlContext.Model.CreatePredictionEngine<SongRating, SongRatingPrediction>(model);

                var predictions = unheardSongIds
                    .Select(songId => new
                    {
                        SongId = songId,
                        Score = predEngine.Predict(new SongRating
                        {
                            UserId = currentUserFloat,
                            SongId = (float)songId,
                            Label = 0
                        }).Score
                    })
                    .Where(p => !float.IsNaN(p.Score))
                    .OrderByDescending(p => p.Score)
                    .Take(topN)
                    .Select(p => p.SongId)
                    .ToList();

                _logger.LogInformation("ML öneri tamamlandı. Kullanıcı: {UserId}, Öneri sayısı: {Count}",
                    userId, predictions.Count);

                return predictions.Any()
                    ? predictions
                    : await GetFallbackRecommendations(userId, topN);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ML öneri hatası. Fallback'e geçiliyor.");
                return await GetFallbackRecommendations(userId, topN);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Fallback: ML çalışamazsa content-based öneri
        // Kullanıcının en çok dinlediği sanatçının diğer şarkılarını öner
        // ══════════════════════════════════════════════════════════════════
        private async Task<List<int>> GetFallbackRecommendations(string userId, int topN)
        {
            // Kullanıcının dinlediği şarkı ID'leri
            var listenedSongIds = await _context.ListeningHistories
                .Where(h => h.AppUserId == userId)
                .Select(h => h.SongId)
                .Distinct()
                .ToListAsync();

            if (!listenedSongIds.Any())
            {
                // Hiç dinlememişse en popüler şarkıları döndür
                return await _context.ListeningHistories
                    .GroupBy(h => h.SongId)
                    .OrderByDescending(g => g.Count())
                    .Take(topN)
                    .Select(g => g.Key)
                    .ToListAsync();
            }

            // En çok dinlenen sanatçıyı bul
            var favoriteArtistId = await _context.ListeningHistories
                .Where(h => h.AppUserId == userId)
                .Include(h => h.Song)
                .GroupBy(h => h.Song.ArtistId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            // O sanatçının dinlenmemiş şarkılarını öner
            var artistSongs = await _context.Songs
                .Where(s => s.ArtistId == favoriteArtistId && !listenedSongIds.Contains(s.SongId))
                .Take(topN / 2)
                .Select(s => s.SongId)
                .ToListAsync();

            // Kalan slotları en popüler dinlenmemiş şarkılarla doldur
            var popularSongs = await _context.ListeningHistories
                .Where(h => !listenedSongIds.Contains(h.SongId))
                .GroupBy(h => h.SongId)
                .OrderByDescending(g => g.Count())
                .Take(topN - artistSongs.Count)
                .Select(g => g.Key)
                .ToListAsync();

            return artistSongs.Union(popularSongs).Take(topN).ToList();
        }
    }
}
