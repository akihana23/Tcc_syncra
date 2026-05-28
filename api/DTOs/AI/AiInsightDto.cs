namespace SocialListening.API.DTOs.AI
{
    public class AiInsightRequestDto
    {
        public string Query { get; set; } = string.Empty;

        public List<AiInsightItemDto> Items { get; set; } = [];
    }

    public class AiInsightItemDto
    {
        public string Source { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Sentiment { get; set; } = "neutral";

        public int Engagement { get; set; }
    }

    public class AiInsightResponseDto
    {
        public bool UsedAi { get; set; }

        public List<string> Suggestions { get; set; } = [];
    }
}
