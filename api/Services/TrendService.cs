using SocialListening.API.DTOs;
using System.Text.RegularExpressions;

namespace SocialListening.API.Services
{
    public class TrendService
    {
        private readonly RedditService _redditService;
        private readonly YoutubeService _youtubeService;
        private readonly BlueskyService _blueskyService;

        public TrendService(
            RedditService redditService,
            YoutubeService youtubeService,
            BlueskyService blueskyService)
        {
            _redditService = redditService;
            _youtubeService = youtubeService;
            _blueskyService = blueskyService;
        }

        public async Task<List<TrendDto>> GetTrends(string query)
        {
            var texts = new List<string>();

            try
            {
                var redditPosts = await _redditService.SearchPosts(query);

                texts.AddRange(
                    redditPosts.Select(p => p.Title)
                );
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is TaskCanceledException ||
                    ex is System.Text.Json.JsonException
                )
            {
                // Trends are best-effort; one source failing should not break the endpoint.
            }

            try
            {
                var youtubeVideos = await _youtubeService.SearchVideos(query);

                texts.AddRange(
                    youtubeVideos.Select(v => v.Title)
                );
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is TaskCanceledException ||
                    ex is InvalidOperationException ||
                    ex is System.Text.Json.JsonException
                )
            {
                // Trends are best-effort; one source failing should not break the endpoint.
            }

            try
            {
                var blueskyPosts = await _blueskyService.SearchPosts(query);

                texts.AddRange(
                    blueskyPosts.Select(p => p.Title)
                );
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is TaskCanceledException ||
                    ex is System.Text.Json.JsonException
                )
            {
                // Trends are best-effort; one source failing should not break the endpoint.
            }

            var stopWords = new List<string>
            {
                "the", "and", "for", "with",
                "this", "that", "from",
                "como", "para", "sobre",
                "você", "porque", "quando",
                "de", "da", "do", "e",
                "a", "o", "em"
            };

            var words = texts
                .SelectMany(text =>
                    Regex.Split(text.ToLower(), @"\W+")
                )
                .Where(word =>
                    word.Length > 3 &&
                    !stopWords.Contains(word)
                );

            var trends = words
                .GroupBy(word => word)
                .Select(group => new TrendDto
                {
                    Keyword = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(t => t.Count)
                .Take(10)
                .ToList();

            return trends;
        }
    }
}
