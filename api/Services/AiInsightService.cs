using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SocialListening.API.DTOs.AI;
using SocialListening.API.DTOs.Brands;

namespace SocialListening.API.Services
{
    public class AiInsightService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiInsightService(
            HttpClient httpClient,
            IConfiguration configuration
        )
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AiInsightResponseDto> GenerateInsights(
            AiInsightRequestDto request
        )
        {
            var apiKey =
                GetApiKey();

            var model =
                GetModel();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Chave da OpenAI não configurada."
                );
            }

            var payload = new
            {
                model,
                instructions = "Você é um analista de social listening para MEI e pequenas empresas. Gere sugestões curtas, práticas e em português do Brasil.",
                input = BuildPrompt(request),
                max_output_tokens = 500
            };

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.openai.com/v1/responses"
                );

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            httpRequest.Content =
                new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

            using var response =
                await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"OpenAI retornou {(int)response.StatusCode} ({response.ReasonPhrase})."
                );
            }

            var json =
                await response.Content.ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(json);

            var text =
                ExtractOutputText(document.RootElement);

            return new AiInsightResponseDto
            {
                UsedAi = true,
                Suggestions = SplitSuggestions(text)
            };
        }

        public async Task<AiInsightResponseDto> GenerateBrandComparisonInsights(
            List<SavedBrandComparisonItemDto> brands
        )
        {
            var apiKey =
                GetApiKey();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Chave da OpenAI não configurada."
                );
            }

            var payload = new
            {
                model = GetModel(),
                instructions = "Você é um analista de social listening para MEI e pequenas empresas. Compare marcas com cuidado, sem inventar dados além do que foi enviado. Responda em português do Brasil.",
                input = BuildComparisonPrompt(brands),
                max_output_tokens = 600
            };

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.openai.com/v1/responses"
                );

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            httpRequest.Content =
                new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

            using var response =
                await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"OpenAI retornou {(int)response.StatusCode} ({response.ReasonPhrase})."
                );
            }

            var json =
                await response.Content.ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(json);

            var text =
                ExtractOutputText(document.RootElement);

            return new AiInsightResponseDto
            {
                UsedAi = true,
                Suggestions = SplitSuggestions(text)
            };
        }

        private string GetApiKey()
        {
            var configuredKey =
                _configuration["OpenAI:ApiKey"];

            if (!string.IsNullOrWhiteSpace(configuredKey))
            {
                return configuredKey;
            }

            return
                Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                string.Empty;
        }

        private string GetModel()
        {
            var model =
                _configuration["OpenAI:Model"];

            if (string.IsNullOrWhiteSpace(model))
            {
                model =
                    Environment.GetEnvironmentVariable("OPENAI_MODEL");
            }

            return string.IsNullOrWhiteSpace(model)
                ? "gpt-5-mini"
                : model;
        }

        private static string BuildPrompt(AiInsightRequestDto request)
        {
            var compactItems =
                request.Items
                    .Take(30)
                    .Select(item => new
                    {
                        item.Source,
                        item.Title,
                        item.Sentiment,
                        item.Engagement
                    });

            return
                "Analise estes resultados e retorne de 3 a 5 sugestões acionáveis, uma por linha, sem texto introdutório.\n\n" +
                JsonSerializer.Serialize(new
                {
                    request.Query,
                    Items = compactItems
                });
        }

        private static string BuildComparisonPrompt(
            List<SavedBrandComparisonItemDto> brands
        )
        {
            var compactBrands =
                brands.Select(item => new
                {
                    item.Brand.Name,
                    item.Brand.Category,
                    item.Brand.City,
                    SnapshotDate = item.Snapshot.CreatedAt,
                    item.Snapshot.TotalMentions,
                    item.Snapshot.Engagement,
                    item.Snapshot.Positive,
                    item.Snapshot.Negative,
                    item.Snapshot.Neutral,
                    TopTerms = item.Snapshot.TopTerms.Take(8),
                    Platforms = item.Snapshot.Platforms,
                    Examples = item.Snapshot.Items
                        .Take(8)
                        .Select(mention => new
                        {
                            mention.Source,
                            mention.Title,
                            mention.Sentiment,
                            mention.Engagement
                        })
                });

            return
                "Compare estes snapshots de marcas e retorne de 3 a 5 insights acionáveis, uma frase por linha. " +
                "Destaque diferença de volume, sentimento, termos recorrentes e uma ação prática para pequeno negócio.\n\n" +
                JsonSerializer.Serialize(new
                {
                    Brands = compactBrands
                });
        }

        private static string ExtractOutputText(JsonElement root)
        {
            if (
                root.TryGetProperty("output_text", out var outputText) &&
                outputText.ValueKind == JsonValueKind.String
            )
            {
                return outputText.GetString() ?? "";
            }

            if (!root.TryGetProperty("output", out var output))
            {
                return "";
            }

            var parts =
                new List<string>();

            foreach (var outputItem in output.EnumerateArray())
            {
                if (!outputItem.TryGetProperty("content", out var content))
                {
                    continue;
                }

                foreach (var contentItem in content.EnumerateArray())
                {
                    if (
                        contentItem.TryGetProperty("text", out var text) &&
                        text.ValueKind == JsonValueKind.String
                    )
                    {
                        parts.Add(text.GetString() ?? "");
                    }
                }
            }

            return string.Join("\n", parts);
        }

        private static List<string> SplitSuggestions(string text)
        {
            return text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line =>
                    line.Trim()
                        .TrimStart('-', '*')
                        .Trim()
                )
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Take(5)
                .ToList();
        }
    }
}
