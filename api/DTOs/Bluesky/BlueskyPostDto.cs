namespace SocialListening.API.DTOs.Bluesky
{
    public class BlueskyPostDto
    {
        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string Handle { get; set; } = string.Empty;

        public int Likes { get; set; }

        public int Replies { get; set; }

        public int Reposts { get; set; }

        public string Url { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public string Sentiment { get; set; } = "neutral";
    }
}
