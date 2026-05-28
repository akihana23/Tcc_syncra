using System.Text.Json;

using SocialListening.API.DTOs.Youtube;

namespace SocialListening.API.Services
{
    public class YoutubeService
    {
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;

        private readonly SentimentService _sentimentService;

        private readonly BrandRelevanceService _brandRelevanceService;

        public YoutubeService(
            HttpClient httpClient,
            IConfiguration configuration,
            SentimentService sentimentService,
            BrandRelevanceService brandRelevanceService
        )
        {
            _httpClient = httpClient;

            _configuration = configuration;

            _sentimentService = sentimentService;

            _brandRelevanceService = brandRelevanceService;
        }

        public async Task<List<YoutubeVideoDto>>
            SearchVideos(string query)
        {
            var apiKey =
                _configuration["Youtube:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Chave da API do YouTube não configurada."
                );
            }

            var externalQuery =
                _brandRelevanceService.BuildExternalQuery(query);

            var encodedQuery =
                Uri.EscapeDataString(externalQuery);

            var url =
                $"https://www.googleapis.com/youtube/v3/search" +
                $"?part=snippet" +
                $"&q={encodedQuery}" +
                $"&type=video" +
                $"&maxResults=25" +
                $"&key={apiKey}";

            var response =
                await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"YouTube retornou {(int)response.StatusCode} ({response.ReasonPhrase})."
                );
            }

            var json =
                await response.Content.ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(json);

            var items =
                document.RootElement
                    .GetProperty("items");

            var results =
                new List<YoutubeVideoDto>();

            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("snippet", out var snippet))
                {
                    continue;
                }

                var title =
                    snippet.TryGetProperty("title", out var titleElement)
                        ? titleElement.GetString() ?? ""
                        : "";

                results.Add(new YoutubeVideoDto
                {
                    Title = title,

                    Channel =
                        snippet.TryGetProperty("channelTitle", out var channelElement)
                            ? channelElement.GetString() ?? ""
                            : "",

                    Thumbnail =
                        GetThumbnailUrl(snippet),

                    VideoId =
                        item.TryGetProperty("id", out var idElement) &&
                        idElement.TryGetProperty("videoId", out var videoIdElement)
                            ? videoIdElement.GetString() ?? ""
                            : "",

                    Sentiment =
                        _sentimentService
                            .Analyze(title)
                });
            }

            return _brandRelevanceService.FilterRelevant(
                results,
                query,
                video => $"{video.Title} {video.Channel}"
            );
        }

        private static string GetThumbnailUrl(JsonElement snippet)
        {
            if (!snippet.TryGetProperty("thumbnails", out var thumbnails))
            {
                return "";
            }

            foreach (var quality in new[] { "high", "medium", "default" })
            {
                if (
                    thumbnails.TryGetProperty(quality, out var thumbnail) &&
                    thumbnail.TryGetProperty("url", out var url)
                )
                {
                    return url.GetString() ?? "";
                }
            }

            return "";
        }
    }
}
