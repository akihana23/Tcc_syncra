using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using SocialListening.API.DTOs.Reddit;

namespace SocialListening.API.Services
{
    public class RedditService
    {
        private const string TokenUrl =
            "https://www.reddit.com/api/v1/access_token";

        private const string PublicSearchUrl =
            "https://www.reddit.com/search.json";

        private const string OAuthSearchUrl =
            "https://oauth.reddit.com/search";

        private readonly HttpClient _httpClient;

        private readonly SentimentService _sentimentService;

        private readonly BrandRelevanceService _brandRelevanceService;

        private readonly IConfiguration _configuration;

        private readonly SemaphoreSlim _tokenLock = new(1, 1);

        private readonly string _userAgent;

        private string? _accessToken;

        private DateTimeOffset _accessTokenExpiresAt =
            DateTimeOffset.MinValue;

        public RedditService(
            HttpClient httpClient,
            SentimentService sentimentService,
            BrandRelevanceService brandRelevanceService,
            IConfiguration configuration
        )
        {
            _httpClient = httpClient;

            _sentimentService = sentimentService;

            _brandRelevanceService = brandRelevanceService;

            _configuration = configuration;

            _userAgent =
                GetConfigurationValue("Reddit:UserAgent", "REDDIT_USER_AGENT") ??
                "SyncraSocialListening/1.0 by tcc-syncra";

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            _httpClient.Timeout = TimeSpan.FromSeconds(20);
        }

        public async Task<List<RedditPostDto>>
            SearchPosts(string query)
        {
            var externalQuery =
                _brandRelevanceService.BuildExternalQuery(query);

            var encodedQuery =
                Uri.EscapeDataString(externalQuery);

            var json =
                await FetchSearchJson(encodedQuery);

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
                        BuildRedditUrl(data),

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

        private async Task<string> FetchSearchJson(string encodedQuery)
        {
            if (HasOAuthConfiguration())
            {
                return await FetchSearchJsonWithOAuth(encodedQuery);
            }

            try
            {
                var url =
                    $"{PublicSearchUrl}?q={encodedQuery}&limit=50&raw_json=1";

                using var request =
                    new HttpRequestMessage(HttpMethod.Get, url);

                using var response =
                    await _httpClient.SendAsync(request);

                return await ReadRedditResponse(response);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    "O Reddit bloqueou buscas sem autenticacao. Configure Reddit:ClientId e Reddit:ClientSecret para usar OAuth.",
                    ex
                );
            }
        }

        private async Task<string> FetchSearchJsonWithOAuth(
            string encodedQuery
        )
        {
            var token =
                await GetAccessToken();

            var response =
                await SendOAuthSearchRequest(encodedQuery, token);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();

                _accessToken = null;
                _accessTokenExpiresAt = DateTimeOffset.MinValue;

                token =
                    await GetAccessToken();

                response =
                    await SendOAuthSearchRequest(encodedQuery, token);
            }

            using (response)
            {
                return await ReadRedditResponse(response);
            }
        }

        private async Task<HttpResponseMessage> SendOAuthSearchRequest(
            string encodedQuery,
            string token
        )
        {
            var url =
                $"{OAuthSearchUrl}?q={encodedQuery}&limit=50&sort=relevance&type=link&raw_json=1";

            using var request =
                new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request);
        }

        private async Task<string> GetAccessToken()
        {
            if (
                !string.IsNullOrWhiteSpace(_accessToken) &&
                DateTimeOffset.UtcNow < _accessTokenExpiresAt
            )
            {
                return _accessToken;
            }

            await _tokenLock.WaitAsync();

            try
            {
                if (
                    !string.IsNullOrWhiteSpace(_accessToken) &&
                    DateTimeOffset.UtcNow < _accessTokenExpiresAt
                )
                {
                    return _accessToken;
                }

                var clientId =
                    GetConfigurationValue(
                        "Reddit:ClientId",
                        "REDDIT_CLIENT_ID"
                    );

                var clientSecret =
                    GetConfigurationValue(
                        "Reddit:ClientSecret",
                        "REDDIT_CLIENT_SECRET"
                    );

                if (
                    string.IsNullOrWhiteSpace(clientId) ||
                    string.IsNullOrWhiteSpace(clientSecret)
                )
                {
                    throw new InvalidOperationException(
                        "Configure Reddit:ClientId e Reddit:ClientSecret para buscar dados do Reddit."
                    );
                }

                var credentials =
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}")
                    );

                using var request =
                    new HttpRequestMessage(HttpMethod.Post, TokenUrl);

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);

                request.Content =
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            ["grant_type"] = "client_credentials"
                        }
                    );

                using var response =
                    await _httpClient.SendAsync(request);

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Reddit nao autorizou a aplicacao ({(int)response.StatusCode}: {ExtractFailureReason(response, responseBody)})."
                    );
                }

                using var document =
                    JsonDocument.Parse(responseBody);

                _accessToken =
                    document.RootElement
                        .GetProperty("access_token")
                        .GetString();

                var expiresIn =
                    document.RootElement.TryGetProperty(
                        "expires_in",
                        out var expiresInElement
                    )
                        ? expiresInElement.GetInt32()
                        : 3600;

                _accessTokenExpiresAt =
                    DateTimeOffset.UtcNow.AddSeconds(
                        Math.Max(expiresIn - 60, 60)
                    );

                return _accessToken ??
                    throw new HttpRequestException(
                        "Reddit nao retornou um token de acesso."
                    );
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private async Task<string> ReadRedditResponse(
            HttpResponseMessage response
        )
        {
            var body =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Reddit retornou {(int)response.StatusCode} ({ExtractFailureReason(response, body)})."
                );
            }

            return body;
        }

        private bool HasOAuthConfiguration()
        {
            return
                !string.IsNullOrWhiteSpace(
                    GetConfigurationValue(
                        "Reddit:ClientId",
                        "REDDIT_CLIENT_ID"
                    )
                ) &&
                !string.IsNullOrWhiteSpace(
                    GetConfigurationValue(
                        "Reddit:ClientSecret",
                        "REDDIT_CLIENT_SECRET"
                    )
                );
        }

        private string? GetConfigurationValue(
            string configurationKey,
            string environmentKey
        )
        {
            var value =
                _configuration[configurationKey] ??
                Environment.GetEnvironmentVariable(environmentKey);

            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string BuildRedditUrl(JsonElement data)
        {
            var permalink =
                data.TryGetProperty("permalink", out var permalinkElement)
                    ? permalinkElement.GetString()
                    : null;

            if (string.IsNullOrWhiteSpace(permalink))
            {
                return "";
            }

            if (Uri.TryCreate(permalink, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            return $"https://reddit.com{permalink}";
        }

        private static string ExtractFailureReason(
            HttpResponseMessage response,
            string body
        )
        {
            if (body.Contains("You've been blocked by network security"))
            {
                return "Blocked";
            }

            try
            {
                using var document =
                    JsonDocument.Parse(body);

                if (
                    document.RootElement.TryGetProperty(
                        "message",
                        out var message
                    )
                )
                {
                    return message.GetString() ?? response.ReasonPhrase ?? "Erro";
                }

                if (
                    document.RootElement.TryGetProperty(
                        "error",
                        out var error
                    )
                )
                {
                    return error.GetString() ?? response.ReasonPhrase ?? "Erro";
                }
            }
            catch (JsonException)
            {
            }

            return response.ReasonPhrase ?? "Erro";
        }
    }
}
