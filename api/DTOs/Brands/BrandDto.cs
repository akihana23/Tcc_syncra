using SocialListening.API.DTOs.Analysis;

namespace SocialListening.API.DTOs.Brands
{
    public class BrandUpsertDto
    {
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Aliases { get; set; } = string.Empty;

        public string Competitors { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }

    public class BrandSnapshotCreateDto
    {
        public string Query { get; set; } = string.Empty;

        public int TotalMentions { get; set; }

        public int Engagement { get; set; }

        public int Positive { get; set; }

        public int Negative { get; set; }

        public int Neutral { get; set; }

        public List<PlatformBreakdownDto> Platforms { get; set; } = [];

        public List<AnalysisTermDto> TopTerms { get; set; } = [];

        public List<SocialListeningItemDto> Items { get; set; } = [];

        public List<string> Recommendations { get; set; } = [];

        public List<string> AiInsights { get; set; } = [];
    }

    public class BrandDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Aliases { get; set; } = string.Empty;

        public string Competitors { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int SnapshotCount { get; set; }

        public BrandSnapshotDto? LatestSnapshot { get; set; }
    }

    public class BrandSnapshotDto
    {
        public int Id { get; set; }

        public int BrandProfileId { get; set; }

        public string Query { get; set; } = string.Empty;

        public int TotalMentions { get; set; }

        public int Engagement { get; set; }

        public int Positive { get; set; }

        public int Negative { get; set; }

        public int Neutral { get; set; }

        public List<PlatformBreakdownDto> Platforms { get; set; } = [];

        public List<AnalysisTermDto> TopTerms { get; set; } = [];

        public List<SocialListeningItemDto> Items { get; set; } = [];

        public List<string> Recommendations { get; set; } = [];

        public List<string> AiInsights { get; set; } = [];

        public DateTime CreatedAt { get; set; }
    }

    public class SavedBrandComparisonRequestDto
    {
        public List<int> BrandIds { get; set; } = [];

        public bool UseAi { get; set; } = true;
    }

    public class SavedBrandComparisonItemDto
    {
        public BrandDto Brand { get; set; } = new();

        public BrandSnapshotDto Snapshot { get; set; } = new();
    }

    public class SavedBrandComparisonResponseDto
    {
        public DateTimeOffset GeneratedAt { get; set; }

        public List<SavedBrandComparisonItemDto> Brands { get; set; } = [];

        public List<string> Recommendations { get; set; } = [];

        public bool UsedAi { get; set; }

        public string AiStatus { get; set; } = string.Empty;

        public List<string> AiInsights { get; set; } = [];
    }
}
