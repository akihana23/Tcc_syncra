namespace SocialListening.API.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }

        public string Query { get; set; }
            = string.Empty;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}