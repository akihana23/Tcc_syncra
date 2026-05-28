using System.Text.Json;
using SocialListening.API.DTOs.Bluesky;

namespace SocialListening.API.Services
{
    public class BlueskyService
    {
        private static readonly string[] AppViewHosts =
        [
            "https://public.api.bsky.app",
            "https://api.bsky.app"
        ];

        private readonly HttpClient _httpClient;
        private readonly SentimentService _sentimentService;
        private readonly BrandRelevanceService _brandRelevanceService;

        public BlueskyService(
            HttpClient httpClient,
            SentimentService sentimentService,
            BrandRelevanceService brandRelevanceService
        )
        {
            _httpClient = httpClient;
            _sentimentService = sentimentService;
            _brandRelevanceService = brandRelevanceService;

            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "SocialListeningApp/1.0"
                );
            }
        }

        public async Task<List<BlueskyPostDto>> SearchPosts(string query)
        {
            var externalQuery =
                _brandRelevanceService.BuildExternalQuery(query);

            var encodedQuery =
                Uri.EscapeDataString(externalQuery);

            var json =
                await GetSearchJson(encodedQuery);

            using var document =
                JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("posts", out var posts))
            {
                return [];
            }

            var results =
                new List<BlueskyPostDto>();

            foreach (var post in posts.EnumerateArray())
            {
                var title =
                    GetPostText(post);

                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                var handle =
                    GetAuthorHandle(post);

                results.Add(new BlueskyPostDto
                {
                    Title = title,
                    Author = GetAuthorName(post),
                    Handle = handle,
                    Likes = GetInt32(post, "likeCount"),
                    Replies = GetInt32(post, "replyCount"),
                    Reposts = GetInt32(post, "repostCount"),
                    Url = BuildPostUrl(post, handle),
                    CreatedAt = GetCreatedAt(post),
                    Sentiment = _sentimentService.Analyze(title)
                });
            }

            return _brandRelevanceService.FilterRelevant(
                results,
                query,
                post => $"{post.Title} {post.Author} {post.Handle}"
            );
        }

        private async Task<string> GetSearchJson(string encodedQuery)
        {
            var errors =
                new List<string>();

            foreach (var host in AppViewHosts)
            {
                var url =
                    $"{host}/xrpc/app.bsky.feed.searchPosts" +
                    $"?q={encodedQuery}" +
                    "&sort=latest" +
                    "&limit=50";

                using var response =
                    await _httpClient.GetAsync(url);

                var body =
                    await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return body;
                }

                errors.Add(
                    $"{host} retornou {(int)response.StatusCode} ({response.ReasonPhrase})"
                );
            }

            throw new HttpRequestException(
                $"BlueSky não respondeu com sucesso. {string.Join(" | ", errors)}."
            );
        }

        private static string GetPostText(JsonElement post)
        {
            if (
                post.TryGetProperty("record", out var record) &&
                record.TryGetProperty("text", out var text)
            )
            {
                return text.GetString() ?? "";
            }

            return "";
        }

        private static string GetAuthorName(JsonElement post)
        {
            if (!post.TryGetProperty("author", out var author))
            {
                return "";
            }

            if (author.TryGetProperty("displayName", out var displayName))
            {
                var value =
                    displayName.GetString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return GetAuthorHandle(post);
        }

        private static string GetAuthorHandle(JsonElement post)
        {
            if (
                post.TryGetProperty("author", out var author) &&
                author.TryGetProperty("handle", out var handle)
            )
            {
                return handle.GetString() ?? "";
            }

            return "";
        }

        private static int GetInt32(JsonElement post, string propertyName)
        {
            return
                post.TryGetProperty(propertyName, out var value) &&
                value.TryGetInt32(out var result)
                ? result
                : 0;
        }

        private static DateTime? GetCreatedAt(JsonElement post)
        {
            if (
                post.TryGetProperty("record", out var record) &&
                record.TryGetProperty("createdAt", out var createdAt) &&
                DateTime.TryParse(createdAt.GetString(), out var date)
            )
            {
                return date;
            }

            return null;
        }

        private static string BuildPostUrl(JsonElement post, string handle)
        {
            if (
                string.IsNullOrWhiteSpace(handle) ||
                !post.TryGetProperty("uri", out var uriElement)
            )
            {
                return "https://bsky.app";
            }

            var uri =
                uriElement.GetString() ?? "";

            var postId =
                uri.Split('/').LastOrDefault() ?? "";

            return string.IsNullOrWhiteSpace(postId)
                ? $"https://bsky.app/profile/{handle}"
                : $"https://bsky.app/profile/{handle}/post/{postId}";
        }
    }
}
