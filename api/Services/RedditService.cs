using System.Text.Json;

using SocialListening.API.DTOs.Reddit;

namespace SocialListening.API.Services
{
    public class RedditService
    {
        private readonly HttpClient _httpClient;

        private readonly SentimentService _sentimentService;

        private readonly BrandRelevanceService _brandRelevanceService;

        public RedditService(
            HttpClient httpClient,
            SentimentService sentimentService,
            BrandRelevanceService brandRelevanceService
        )
        {
            _httpClient = httpClient;

            _sentimentService = sentimentService;

            _brandRelevanceService = brandRelevanceService;

            _httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "SocialListeningApp/1.0"
            );
        }

        public async Task<List<RedditPostDto>>
            SearchPosts(string query)
        {
            var externalQuery =
                _brandRelevanceService.BuildExternalQuery(query);

            var encodedQuery =
                Uri.EscapeDataString(externalQuery);

            var url =
                $"https://www.reddit.com/search.json?q={encodedQuery}&limit=50";

            var response =
                await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Reddit retornou {(int)response.StatusCode} ({response.ReasonPhrase})."
                );
            }

            var json =
                await response.Content.ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(json);

            var posts =
                document.RootElement
                    .GetProperty("data")
                    .GetProperty("children");

            var results =
                new List<RedditPostDto>();

            foreach (var post in posts.EnumerateArray())
            {
                if (!post.TryGetProperty("data", out var data))
                {
                    continue;
                }

                var title =
                    data.TryGetProperty("title", out var titleElement)
                        ? titleElement.GetString() ?? ""
                        : "";

                results.Add(new RedditPostDto
                {
                    Title = title,

                    Subreddit =
                        data.TryGetProperty("subreddit", out var subredditElement)
                            ? subredditElement.GetString() ?? ""
                            : "",

                    Author =
                        data.TryGetProperty("author", out var authorElement)
                            ? authorElement.GetString() ?? ""
                            : "",

                    Score =
                        data.TryGetProperty("score", out var scoreElement)
                            ? scoreElement.GetInt32()
                            : 0,

                    Comments =
                        data.TryGetProperty("num_comments", out var commentsElement)
                            ? commentsElement.GetInt32()
                            : 0,

                    Url =
                        "https://reddit.com" +
                        (
                            data.TryGetProperty("permalink", out var permalinkElement)
                                ? permalinkElement.GetString()
                                : ""
                        ),

                    Sentiment =
                        _sentimentService
                            .Analyze(title)
                });
            }

            return _brandRelevanceService.FilterRelevant(
                results,
                query,
                post => $"{post.Title} {post.Subreddit} {post.Author}"
            );
        }
    }
}
