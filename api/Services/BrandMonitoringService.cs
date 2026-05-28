using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SocialListening.API.Data;
using SocialListening.API.DTOs.Analysis;
using SocialListening.API.DTOs.Brands;
using SocialListening.API.Models;

namespace SocialListening.API.Services
{
    public class BrandMonitoringService
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        private readonly AppDbContext _context;
        private readonly AiInsightService _aiInsightService;

        public BrandMonitoringService(
            AppDbContext context,
            AiInsightService aiInsightService
        )
        {
            _context = context;
            _aiInsightService = aiInsightService;
        }

        public async Task<List<BrandDto>> GetBrands()
        {
            var brands =
                await _context.BrandProfiles
                    .AsNoTracking()
                    .Include(brand => brand.Snapshots)
                    .OrderBy(brand => brand.Name)
                    .ToListAsync();

            return brands
                .Select(MapBrand)
                .ToList();
        }

        public async Task<BrandDto?> GetBrand(int id)
        {
            var brand =
                await _context.BrandProfiles
                    .AsNoTracking()
                    .Include(item => item.Snapshots)
                    .FirstOrDefaultAsync(item => item.Id == id);

            return brand is null
                ? null
                : MapBrand(brand);
        }

        public async Task<BrandDto> CreateBrand(
            BrandUpsertDto request
        )
        {
            var now =
                DateTime.UtcNow;

            var brand =
                new BrandProfile
                {
                    Name = request.Name.Trim(),
                    Category = request.Category.Trim(),
                    City = request.City.Trim(),
                    Aliases = request.Aliases.Trim(),
                    Competitors = request.Competitors.Trim(),
                    Notes = request.Notes.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now
                };

            _context.BrandProfiles.Add(brand);

            await _context.SaveChangesAsync();

            return MapBrand(brand);
        }

        public async Task<BrandDto?> UpdateBrand(
            int id,
            BrandUpsertDto request
        )
        {
            var brand =
                await _context.BrandProfiles
                    .Include(item => item.Snapshots)
                    .FirstOrDefaultAsync(item => item.Id == id);

            if (brand is null)
            {
                return null;
            }

            brand.Name = request.Name.Trim();
            brand.Category = request.Category.Trim();
            brand.City = request.City.Trim();
            brand.Aliases = request.Aliases.Trim();
            brand.Competitors = request.Competitors.Trim();
            brand.Notes = request.Notes.Trim();
            brand.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapBrand(brand);
        }

        public async Task<bool> DeleteBrand(int id)
        {
            var brand =
                await _context.BrandProfiles
                    .FirstOrDefaultAsync(item => item.Id == id);

            if (brand is null)
            {
                return false;
            }

            _context.BrandProfiles.Remove(brand);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<BrandSnapshotDto?> SaveSnapshot(
            int brandId,
            BrandSnapshotCreateDto request
        )
        {
            var brandExists =
                await _context.BrandProfiles
                    .AnyAsync(brand => brand.Id == brandId);

            if (!brandExists)
            {
                return null;
            }

            var items =
                request.Items;

            var snapshot =
                new BrandSnapshot
                {
                    BrandProfileId = brandId,
                    Query = request.Query.Trim(),
                    TotalMentions = request.TotalMentions > 0
                        ? request.TotalMentions
                        : items.Count,
                    Engagement = request.Engagement > 0
                        ? request.Engagement
                        : items.Sum(item => item.Engagement),
                    Positive = request.Positive > 0
                        ? request.Positive
                        : CountSentiment(items, "positive"),
                    Negative = request.Negative > 0
                        ? request.Negative
                        : CountSentiment(items, "negative"),
                    Neutral = request.Neutral > 0
                        ? request.Neutral
                        : CountSentiment(items, "neutral"),
                    PlatformsJson = Serialize(request.Platforms),
                    TopTermsJson = Serialize(request.TopTerms),
                    ItemsJson = Serialize(items.Take(40).ToList()),
                    RecommendationsJson = Serialize(request.Recommendations),
                    AiInsightsJson = Serialize(request.AiInsights),
                    CreatedAt = DateTime.UtcNow
                };

            _context.BrandSnapshots.Add(snapshot);

            await _context.SaveChangesAsync();

            return MapSnapshot(snapshot);
        }

        public async Task<SavedBrandComparisonResponseDto> CompareSavedBrands(
            SavedBrandComparisonRequestDto request
        )
        {
            var requestedIds =
                request.BrandIds
                    .Distinct()
                    .Take(5)
                    .ToList();

            if (requestedIds.Count < 2)
            {
                throw new InvalidOperationException(
                    "Selecione pelo menos duas marcas salvas para comparar."
                );
            }

            var brands =
                await _context.BrandProfiles
                    .AsNoTracking()
                    .Include(brand => brand.Snapshots)
                    .Where(brand => requestedIds.Contains(brand.Id))
                    .ToListAsync();

            var comparisonItems =
                requestedIds
                    .Select(id => brands.FirstOrDefault(brand => brand.Id == id))
                    .Where(brand => brand is not null)
                    .Select(brand => BuildComparisonItem(brand!))
                    .Where(item => item is not null)
                    .Select(item => item!)
                    .ToList();

            if (comparisonItems.Count < 2)
            {
                throw new InvalidOperationException(
                    "Salve pelo menos um snapshot em duas marcas antes de comparar."
                );
            }

            var response =
                new SavedBrandComparisonResponseDto
                {
                    GeneratedAt = DateTimeOffset.UtcNow,
                    Brands = comparisonItems,
                    Recommendations = BuildLocalComparisonRecommendations(
                        comparisonItems
                    ),
                    UsedAi = false,
                    AiStatus = "IA não solicitada."
                };

            if (!request.UseAi)
            {
                return response;
            }

            try
            {
                var aiInsights =
                    await _aiInsightService
                        .GenerateBrandComparisonInsights(comparisonItems);

                response.UsedAi = true;
                response.AiStatus = "IA ativa.";
                response.AiInsights = aiInsights.Suggestions;
            }
            catch (InvalidOperationException ex)
            {
                response.AiStatus = ex.Message;
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is System.Text.Json.JsonException
                )
            {
                response.AiStatus =
                    $"Não foi possível gerar insights por IA agora: {ex.Message}";
            }

            return response;
        }

        private static SavedBrandComparisonItemDto? BuildComparisonItem(
            BrandProfile brand
        )
        {
            var latestSnapshot =
                brand.Snapshots
                    .OrderByDescending(snapshot => snapshot.CreatedAt)
                    .FirstOrDefault();

            if (latestSnapshot is null)
            {
                return null;
            }

            return new SavedBrandComparisonItemDto
            {
                Brand = MapBrand(brand),
                Snapshot = MapSnapshot(latestSnapshot)
            };
        }

        private static BrandDto MapBrand(BrandProfile brand)
        {
            var snapshots =
                brand.Snapshots
                    .OrderByDescending(snapshot => snapshot.CreatedAt)
                    .ToList();

            return new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Category = brand.Category,
                City = brand.City,
                Aliases = brand.Aliases,
                Competitors = brand.Competitors,
                Notes = brand.Notes,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt,
                SnapshotCount = snapshots.Count,
                LatestSnapshot = snapshots.FirstOrDefault() is { } latest
                    ? MapSnapshot(latest)
                    : null
            };
        }

        private static BrandSnapshotDto MapSnapshot(
            BrandSnapshot snapshot
        )
        {
            return new BrandSnapshotDto
            {
                Id = snapshot.Id,
                BrandProfileId = snapshot.BrandProfileId,
                Query = snapshot.Query,
                TotalMentions = snapshot.TotalMentions,
                Engagement = snapshot.Engagement,
                Positive = snapshot.Positive,
                Negative = snapshot.Negative,
                Neutral = snapshot.Neutral,
                Platforms =
                    DeserializeList<PlatformBreakdownDto>(
                        snapshot.PlatformsJson
                    ),
                TopTerms =
                    DeserializeList<AnalysisTermDto>(
                        snapshot.TopTermsJson
                    ),
                Items =
                    DeserializeList<SocialListeningItemDto>(
                        snapshot.ItemsJson
                    ),
                Recommendations =
                    DeserializeList<string>(
                        snapshot.RecommendationsJson
                    ),
                AiInsights =
                    DeserializeList<string>(
                        snapshot.AiInsightsJson
                    ),
                CreatedAt = snapshot.CreatedAt
            };
        }

        private static List<string> BuildLocalComparisonRecommendations(
            List<SavedBrandComparisonItemDto> comparisonItems
        )
        {
            var recommendations =
                new List<string>();

            var volumeLeader =
                comparisonItems
                    .OrderByDescending(item => item.Snapshot.TotalMentions)
                    .FirstOrDefault();

            if (volumeLeader is not null)
            {
                recommendations.Add(
                    $"{volumeLeader.Brand.Name} concentrou o maior volume salvo, com {volumeLeader.Snapshot.TotalMentions} menções."
                );
            }

            var highestPositive =
                comparisonItems
                    .Where(item => item.Snapshot.TotalMentions > 0)
                    .OrderByDescending(item => PositiveRate(item.Snapshot))
                    .FirstOrDefault();

            if (highestPositive is not null)
            {
                recommendations.Add(
                    $"{highestPositive.Brand.Name} teve a melhor proporção positiva nos snapshots salvos ({PositiveRate(highestPositive.Snapshot):P0})."
                );
            }

            var highestNegative =
                comparisonItems
                    .Where(item => item.Snapshot.TotalMentions > 0)
                    .OrderByDescending(item => NegativeRate(item.Snapshot))
                    .FirstOrDefault();

            if (
                highestNegative is not null &&
                highestNegative.Snapshot.Negative > 0
            )
            {
                recommendations.Add(
                    $"{highestNegative.Brand.Name} teve o maior sinal de risco negativo ({NegativeRate(highestNegative.Snapshot):P0}). Revise os termos e menções antes de concluir."
                );
            }

            recommendations.Add(
                "Compare snapshots feitos em datas próximas para evitar conclusões enviesadas por assuntos passageiros."
            );

            return recommendations;
        }

        private static double PositiveRate(BrandSnapshotDto snapshot)
        {
            return snapshot.TotalMentions == 0
                ? 0
                : (double)snapshot.Positive / snapshot.TotalMentions;
        }

        private static double NegativeRate(BrandSnapshotDto snapshot)
        {
            return snapshot.TotalMentions == 0
                ? 0
                : (double)snapshot.Negative / snapshot.TotalMentions;
        }

        private static int CountSentiment(
            IEnumerable<SocialListeningItemDto> items,
            string sentiment
        )
        {
            return items.Count(item => item.Sentiment == sentiment);
        }

        private static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, JsonOptions);
        }

        private static List<T> DeserializeList<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<T>>(
                    json,
                    JsonOptions
                ) ?? [];
            }
            catch (System.Text.Json.JsonException)
            {
                return [];
            }
        }
    }
}
