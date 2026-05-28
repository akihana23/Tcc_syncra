namespace SocialListening.API.Models
{
    public class BrandSnapshot
    {
        public int Id { get; set; }

        public int BrandProfileId { get; set; }

        public BrandProfile? BrandProfile { get; set; }

        public string Query { get; set; } = string.Empty;

        public int TotalMentions { get; set; }

        public int Engagement { get; set; }

        public int Positive { get; set; }

        public int Negative { get; set; }

        public int Neutral { get; set; }

        public string PlatformsJson { get; set; } = "[]";

        public string TopTermsJson { get; set; } = "[]";

        public string ItemsJson { get; set; } = "[]";

        public string RecommendationsJson { get; set; } = "[]";

        public string AiInsightsJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
