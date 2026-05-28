namespace SocialListening.API.DTOs.Analysis
{
    public class SocialListeningItemDto
    {
        public string Source { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Sentiment { get; set; } = "neutral";

        public int Engagement { get; set; }
    }

    public class AnalysisSourceStatusDto
    {
        public string Source { get; set; } = string.Empty;

        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public class AnalysisTermDto
    {
        public string Term { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class AnalysisBrandMentionDto
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class PlatformBreakdownDto
    {
        public string Source { get; set; } = string.Empty;

        public int Mentions { get; set; }

        public int Engagement { get; set; }

        public int Positive { get; set; }

        public int Negative { get; set; }

        public int Neutral { get; set; }
    }

    public class BrandAnalysisDto
    {
        public string Brand { get; set; } = string.Empty;

        public int TotalMentions { get; set; }

        public int Engagement { get; set; }

        public int Positive { get; set; }

        public int Negative { get; set; }

        public int Neutral { get; set; }

        public List<PlatformBreakdownDto> Platforms { get; set; } = [];

        public List<AnalysisTermDto> TopTerms { get; set; } = [];

        public List<SocialListeningItemDto> Items { get; set; } = [];

        public List<AnalysisSourceStatusDto> SourceStatuses { get; set; } = [];

        public List<string> Recommendations { get; set; } = [];
    }

    public class BrandComparisonResponseDto
    {
        public DateTimeOffset GeneratedAt { get; set; }

        public List<BrandAnalysisDto> Brands { get; set; } = [];

        public List<string> Recommendations { get; set; } = [];
    }

    public class TopicAnalysisResponseDto
    {
        public string Query { get; set; } = string.Empty;

        public DateTimeOffset GeneratedAt { get; set; }

        public int TotalMentions { get; set; }

        public int Engagement { get; set; }

        public List<PlatformBreakdownDto> Platforms { get; set; } = [];

        public List<AnalysisTermDto> TopTerms { get; set; } = [];

        public List<AnalysisBrandMentionDto> MentionedBrands { get; set; } = [];

        public List<SocialListeningItemDto> Items { get; set; } = [];

        public List<AnalysisSourceStatusDto> SourceStatuses { get; set; } = [];

        public List<string> Recommendations { get; set; } = [];
    }

    public class RoadmapItemDto
    {
        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class RoadmapResponseDto
    {
        public DateTimeOffset GeneratedAt { get; set; }

        public List<RoadmapItemDto> Items { get; set; } = [];
    }
}
