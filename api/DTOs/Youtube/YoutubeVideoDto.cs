namespace SocialListening.API.DTOs.Youtube
{
    public class YoutubeVideoDto
    {
        public string Title { get; set; } = string.Empty;

        public string Channel { get; set; } = string.Empty;

        public string Thumbnail { get; set; } = string.Empty;

        public string VideoId { get; set; } = string.Empty;

        public string Sentiment { get; set; } = "neutral";
        public string Url =>
            $"https://youtube.com/watch?v={VideoId}";

        
    }
}