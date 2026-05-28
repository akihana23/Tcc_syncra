namespace SocialListening.API.Models
{
    public class BrandProfile
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Aliases { get; set; } = string.Empty;

        public string Competitors { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<BrandSnapshot> Snapshots { get; set; } = [];
    }
}
