using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using SocialListening.API.DTOs.Mastodon;

namespace SocialListening.API.Services
{
    public class MastodonService
    {
        private static readonly string[] DefaultInstances =
        [
            "https://mastodon.social",
            "https://fosstodon.org",
            "https://mastodon.world"
        ];

        private readonly HttpClient _httpClient;
        private readonly SentimentService _sentimentService;
        private readonly IConfiguration _configuration;

        public MastodonService(
            HttpClient httpClient,
            SentimentService sentimentService,
            IConfiguration configuration
        )
        {
            _httpClient = httpClient;
            _sentimentService = sentimentService;
            _configuration = configuration;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "SyncraSocialListening/1.0"
                );
            }
        }

        public async Task<List<MastodonPostDto>> SearchPosts(string query)
        {
            var hashtag = NormalizeHashtag(query);

            if (string.IsNullOrWhiteSpace(hashtag))
            {
                throw new InvalidOperationException(
                    "Informe uma hashtag ou um termo formado por letras e numeros para buscar no Mastodon."
                );
            }

            var tasks = GetInstances()
                .Select(instance => GetPostsFromInstance(instance, hashtag));

            var responses = await Task.WhenAll(tasks);

            var successfulResponses = responses
                .Where(response => response.Error is null)
                .ToList();

            if (successfulResponses.Count == 0)
            {
                var errors = responses
                    .Select(response => response.Error)
                    .Where(error => !string.IsNullOrWhiteSpace(error));

                throw new HttpRequestException(
                    $"Mastodon nao respondeu com sucesso. {string.Join(" | ", errors)}"
                );
            }

            return successfulResponses
                .SelectMany(response => response.Posts)
                .GroupBy(post => post.Url, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderByDescending(post => post.CreatedAt)
                .Take(50)
                .ToList();
        }

        private async Task<InstanceResponse> GetPostsFromInstance(
            string instance,
            string hashtag
        )
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));

            try
            {
                var baseUrl = instance.TrimEnd('/');
                var encodedTag = Uri.EscapeDataString(hashtag);
                var url = $"{baseUrl}/api/v1/timelines/tag/{encodedTag}?limit=40";

                using var request =
                    new HttpRequestMessage(HttpMethod.Get, url);

                var accessToken =
                    _configuration["Mastodon:AccessToken"] ??
                    Environment.GetEnvironmentVariable("MASTODON_ACCESS_TOKEN");

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            accessToken.Trim()
                        );
                }

                using var response =
                    await _httpClient.SendAsync(request, cts.Token);

                var body =
                    await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    return new InstanceResponse(
                        [],
                        $"{baseUrl} retornou {(int)response.StatusCode} ({response.ReasonPhrase})"
                    );
                }

                using var document = JsonDocument.Parse(body);

                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return new InstanceResponse([], $"{baseUrl} retornou um formato invalido.");
                }

                var posts = document.RootElement
                    .EnumerateArray()
                    .Select(MapPost)
                    .Where(post => !string.IsNullOrWhiteSpace(post.Title))
                    .ToList();

                return new InstanceResponse(posts, null);
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is TaskCanceledException ||
                    ex is OperationCanceledException ||
                    ex is JsonException
                )
            {
                return new InstanceResponse([], $"{instance}: {ex.Message}");
            }
        }

        private MastodonPostDto MapPost(JsonElement post)
        {
            var content = GetString(post, "content");
            var title = ToPlainText(content);

            if (post.TryGetProperty("reblog", out var reblog) &&
                reblog.ValueKind == JsonValueKind.Object)
            {
                post = reblog;
                title = ToPlainText(GetString(post, "content"));
            }

            var account =
                post.TryGetProperty("account", out var accountElement)
                    ? accountElement
                    : default;

            var handle = account.ValueKind == JsonValueKind.Object
                ? GetString(account, "acct")
                : string.Empty;

            var author = account.ValueKind == JsonValueKind.Object
                ? GetString(account, "display_name")
                : string.Empty;

            return new MastodonPostDto
            {
                Title = title,
                Author = string.IsNullOrWhiteSpace(author) ? handle : ToPlainText(author),
                Handle = handle,
                Favourites = GetInt32(post, "favourites_count"),
                Boosts = GetInt32(post, "reblogs_count"),
                Replies = GetInt32(post, "replies_count"),
                Url = GetString(post, "url"),
                CreatedAt = GetDateTime(post, "created_at"),
                Sentiment = _sentimentService.Analyze(title)
            };
        }

        private IEnumerable<string> GetInstances()
        {
            var configuredInstances =
                _configuration
                    .GetSection("Mastodon:Instances")
                    .Get<string[]>();

            var environmentInstances =
                Environment.GetEnvironmentVariable("MASTODON_INSTANCES");

            var instances =
                !string.IsNullOrWhiteSpace(environmentInstances)
                    ? environmentInstances.Split(
                        [',', ';'],
                        StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries
                    )
                    : configuredInstances?.Length > 0
                        ? configuredInstances
                        : DefaultInstances;

            return instances
                .Where(instance => Uri.TryCreate(instance, UriKind.Absolute, out _))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string NormalizeHashtag(string query)
        {
            // Remove o '#' inicial se houver e limpa os espaços
            var trimmed = query.Trim().TrimStart('#');

            // Se o termo tem espaço (ex: "minha marca"), usa apenas a primeira palavra
            // pois a API do Mastodon /timelines/tag/ aceita somente uma hashtag por vez
            var firstWord = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? string.Empty;

            return string.Concat(
                firstWord.Where(character =>
                    char.IsLetterOrDigit(character) ||
                    character == '_'
                )
            );
        }

        private static string GetString(JsonElement element, string property)
        {
            return element.TryGetProperty(property, out var value)
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }

        private static int GetInt32(JsonElement element, string property)
        {
            return element.TryGetProperty(property, out var value) &&
                value.TryGetInt32(out var result)
                    ? result
                    : 0;
        }

        private static DateTime? GetDateTime(JsonElement element, string property)
        {
            return DateTime.TryParse(GetString(element, property), out var value)
                ? value
                : null;
        }

        private static string ToPlainText(string html)
        {
            var withoutTags = Regex.Replace(html, "<.*?>", " ");

            return WebUtility.HtmlDecode(withoutTags)
                .Replace("\\n", " ")
                .Trim();
        }

        private record InstanceResponse(
            List<MastodonPostDto> Posts,
            string? Error
        );
    }
}

