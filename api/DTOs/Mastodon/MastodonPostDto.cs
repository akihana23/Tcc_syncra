namespace SocialListening.API.DTOs.Mastodon
{
    public class MastodonPostDto
    {
        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string Handle { get; set; } = string.Empty;

        public int Favourites { get; set; }

        public int Boosts { get; set; }

        public int Replies { get; set; }

        public string Url { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public string Sentiment { get; set; } = "neutral";
    }
}

