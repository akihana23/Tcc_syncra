namespace SocialListening.API.DTOs.Reddit
{
    public class RedditPostDto
    {
        public string Title { get; set; } = string.Empty;

        public string Subreddit { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public int Score { get; set; }

        public int Comments { get; set; }

        public string Url { get; set; } = string.Empty;

        public string Sentiment { get; set; } = "neutral";
    }
}