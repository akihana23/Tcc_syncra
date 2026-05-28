using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SocialListening.API.DTOs.Analysis;
using SocialListening.API.DTOs.Bluesky;
using SocialListening.API.DTOs.Reddit;
using SocialListening.API.DTOs.Youtube;

namespace SocialListening.API.Services
{
    public class SocialListeningAnalysisService
    {
        private static readonly HashSet<string> StopWords = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "a", "as", "o", "os", "um", "uma", "uns", "umas",
            "de", "da", "do", "das", "dos", "em", "no", "na",
            "nos", "nas", "e", "ou", "com", "para", "por",
            "the", "and", "for", "with", "this", "that", "from",
            "sobre", "como", "mais", "muito", "todo", "toda",
            "tem", "ser", "foi", "sao", "são", "esta", "está",
            "esse", "essa", "isso", "pelo", "pela", "pelos",
            "pelas", "que", "pra", "pro", "dos", "das"
        };

        private static readonly string[] KnownBrandHints =
        [
            "tim", "vivo", "claro", "oi", "netflix", "ifood",
            "nubank", "itau", "itaú", "bradesco", "santander",
            "magalu", "mercado livre", "amazon", "americanas",
            "coca cola", "coca-cola", "burger king", "mcdonalds",
            "apple", "samsung", "motorola", "xiaomi"
        ];

        private readonly RedditService _redditService;
        private readonly YoutubeService _youtubeService;
        private readonly BlueskyService _blueskyService;

        public SocialListeningAnalysisService(
            RedditService redditService,
            YoutubeService youtubeService,
            BlueskyService blueskyService
        )
        {
            _redditService = redditService;
            _youtubeService = youtubeService;
            _blueskyService = blueskyService;
        }

        public async Task<TopicAnalysisResponseDto> AnalyzeTopic(
            string query,
            IEnumerable<string>? knownBrands = null
        )
        {
            var fetch =
                await FetchAllSources(query);

            var items =
                fetch.Items
                    .OrderByDescending(item => item.Engagement)
                    .ToList();

            return new TopicAnalysisResponseDto
            {
                Query = query.Trim(),
                GeneratedAt = DateTimeOffset.UtcNow,
                TotalMentions = items.Count,
                Engagement = items.Sum(item => item.Engagement),
                Platforms = BuildPlatformBreakdown(items),
                TopTerms = BuildTopTerms(items),
                MentionedBrands = BuildMentionedBrands(items, knownBrands),
                Items = items.Take(30).ToList(),
                SourceStatuses = fetch.Statuses,
                Recommendations = BuildTopicRecommendations(query, items, fetch.Statuses)
            };
        }

        public async Task<BrandComparisonResponseDto> CompareBrands(
            IEnumerable<string> brands
        )
        {
            var normalizedBrands =
                brands
                    .SelectMany(SplitBrandInput)
                    .Select(brand => brand.Trim())
                    .Where(brand => !string.IsNullOrWhiteSpace(brand))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(5)
                    .ToList();

            var results =
                new List<BrandAnalysisDto>();

            foreach (var brand in normalizedBrands)
            {
                var fetch =
                    await FetchAllSources(brand);

                var items =
                    fetch.Items
                        .OrderByDescending(item => item.Engagement)
                        .ToList();

                results.Add(new BrandAnalysisDto
                {
                    Brand = brand,
                    TotalMentions = items.Count,
                    Engagement = items.Sum(item => item.Engagement),
                    Positive = CountSentiment(items, "positive"),
                    Negative = CountSentiment(items, "negative"),
                    Neutral = CountSentiment(items, "neutral"),
                    Platforms = BuildPlatformBreakdown(items),
                    TopTerms = BuildTopTerms(items, brand),
                    Items = items.Take(20).ToList(),
                    SourceStatuses = fetch.Statuses,
                    Recommendations = BuildBrandRecommendations(brand, items, fetch.Statuses)
                });
            }

            return new BrandComparisonResponseDto
            {
                GeneratedAt = DateTimeOffset.UtcNow,
                Brands = results,
                Recommendations = BuildComparisonRecommendations(results)
            };
        }

        private async Task<FetchAggregate> FetchAllSources(string query)
        {
            var items =
                new List<SocialListeningItemDto>();

            var statuses =
                new List<AnalysisSourceStatusDto>();

            await FetchSource(
                "Reddit",
                async () => (await _redditService.SearchPosts(query))
                    .Select(MapRedditPost),
                items,
                statuses
            );

            await FetchSource(
                "YouTube",
                async () => (await _youtubeService.SearchVideos(query))
                    .Select(MapYoutubeVideo),
                items,
                statuses
            );

            await FetchSource(
                "BlueSky",
                async () => (await _blueskyService.SearchPosts(query))
                    .Select(MapBlueskyPost),
                items,
                statuses
            );

            return new FetchAggregate(items, statuses);
        }

        private static async Task FetchSource(
            string source,
            Func<Task<IEnumerable<SocialListeningItemDto>>> fetch,
            List<SocialListeningItemDto> items,
            List<AnalysisSourceStatusDto> statuses
        )
        {
            try
            {
                var sourceItems =
                    (await fetch()).ToList();

                items.AddRange(sourceItems);

                statuses.Add(new AnalysisSourceStatusDto
                {
                    Source = source,
                    Success = true,
                    Message = sourceItems.Count == 0
                        ? "Nenhum resultado relevante encontrado."
                        : $"{sourceItems.Count} resultados relevantes."
                });
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is InvalidOperationException ||
                    ex is TaskCanceledException ||
                    ex is System.Text.Json.JsonException
                )
            {
                statuses.Add(new AnalysisSourceStatusDto
                {
                    Source = source,
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        private static SocialListeningItemDto MapRedditPost(
            RedditPostDto post
        )
        {
            return new SocialListeningItemDto
            {
                Source = "Reddit",
                Title = post.Title,
                Url = post.Url,
                Sentiment = post.Sentiment,
                Engagement = post.Score + post.Comments
            };
        }

        private static SocialListeningItemDto MapYoutubeVideo(
            YoutubeVideoDto video
        )
        {
            return new SocialListeningItemDto
            {
                Source = "YouTube",
                Title = video.Title,
                Url = string.IsNullOrWhiteSpace(video.VideoId)
                    ? ""
                    : $"https://www.youtube.com/watch?v={video.VideoId}",
                Sentiment = video.Sentiment,
                Engagement = 1
            };
        }

        private static SocialListeningItemDto MapBlueskyPost(
            BlueskyPostDto post
        )
        {
            return new SocialListeningItemDto
            {
                Source = "BlueSky",
                Title = post.Title,
                Url = post.Url,
                Sentiment = post.Sentiment,
                Engagement = post.Likes + post.Replies + post.Reposts
            };
        }

        private static List<PlatformBreakdownDto> BuildPlatformBreakdown(
            List<SocialListeningItemDto> items
        )
        {
            return items
                .GroupBy(item => item.Source)
                .Select(group => new PlatformBreakdownDto
                {
                    Source = group.Key,
                    Mentions = group.Count(),
                    Engagement = group.Sum(item => item.Engagement),
                    Positive = CountSentiment(group, "positive"),
                    Negative = CountSentiment(group, "negative"),
                    Neutral = CountSentiment(group, "neutral")
                })
                .OrderByDescending(platform => platform.Mentions)
                .ToList();
        }

        private static List<AnalysisTermDto> BuildTopTerms(
            List<SocialListeningItemDto> items,
            string? query = null
        )
        {
            var queryTokens =
                string.IsNullOrWhiteSpace(query)
                    ? []
                    : Tokenize(Normalize(query)).ToHashSet(
                        StringComparer.OrdinalIgnoreCase
                    );

            return items
                .SelectMany(item => Tokenize(Normalize(item.Title)))
                .Where(term =>
                    term.Length > 3 &&
                    !StopWords.Contains(term) &&
                    !queryTokens.Contains(term)
                )
                .GroupBy(term => term)
                .Select(group => new AnalysisTermDto
                {
                    Term = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(term => term.Count)
                .ThenBy(term => term.Term)
                .Take(12)
                .ToList();
        }

        private static List<AnalysisBrandMentionDto> BuildMentionedBrands(
            List<SocialListeningItemDto> items,
            IEnumerable<string>? knownBrands
        )
        {
            var hints =
                KnownBrandHints
                    .Concat(knownBrands ?? [])
                    .Select(Normalize)
                    .Where(hint => hint.Length > 1)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            var mentions =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                var normalizedTitle =
                    Normalize(item.Title);

                foreach (var hint in hints)
                {
                    if (ContainsTerm(normalizedTitle, hint))
                    {
                        mentions[hint] = mentions.GetValueOrDefault(hint) + 1;
                    }
                }
            }

            var inferredNames =
                items
                    .SelectMany(item => ExtractCapitalizedNames(item.Title))
                    .Where(name => name.Length > 2)
                    .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .Select(group => new AnalysisBrandMentionDto
                    {
                        Name = group.Key,
                        Count = group.Count()
                    });

            var hintedNames =
                mentions.Select(mention => new AnalysisBrandMentionDto
                {
                    Name = mention.Key,
                    Count = mention.Value
                });

            return hintedNames
                .Concat(inferredNames)
                .GroupBy(mention => mention.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => new AnalysisBrandMentionDto
                {
                    Name = group.Key,
                    Count = group.Sum(item => item.Count)
                })
                .OrderByDescending(mention => mention.Count)
                .ThenBy(mention => mention.Name)
                .Take(12)
                .ToList();
        }

        private static List<string> BuildTopicRecommendations(
            string query,
            List<SocialListeningItemDto> items,
            List<AnalysisSourceStatusDto> statuses
        )
        {
            var recommendations =
                new List<string>();

            if (items.Count == 0)
            {
                recommendations.Add(
                    "Refine a busca com nome da marca, cidade, categoria ou produto para reduzir resultados genericos."
                );
                recommendations.Add(
                    "Cadastre aliases e termos de contexto antes de analisar marcas curtas ou ambiguas."
                );

                return recommendations;
            }

            var topPlatform =
                BuildPlatformBreakdown(items).FirstOrDefault();

            var topTerm =
                BuildTopTerms(items).FirstOrDefault();

            var negativeCount =
                CountSentiment(items, "negative");

            if (topPlatform is not null)
            {
                recommendations.Add(
                    $"Priorize {topPlatform.Source}, que concentrou {topPlatform.Mentions} mencoes nesta busca."
                );
            }

            if (topTerm is not null)
            {
                recommendations.Add(
                    $"Use o termo #{topTerm.Term} como pista para conteudo, produto ou problema recorrente."
                );
            }

            if (negativeCount > 0)
            {
                recommendations.Add(
                    $"Existem {negativeCount} mencoes negativas. Separe os casos por tema antes de responder publicamente."
                );
            }

            AddSourceFailureRecommendation(recommendations, statuses);

            recommendations.Add(
                $"Para validar '{query}', compare com 2 ou 3 marcas concorrentes e observe diferenca de volume, sentimento e termos."
            );

            return recommendations.Take(5).ToList();
        }

        private static List<string> BuildBrandRecommendations(
            string brand,
            List<SocialListeningItemDto> items,
            List<AnalysisSourceStatusDto> statuses
        )
        {
            var recommendations =
                new List<string>();

            if (items.Count == 0)
            {
                recommendations.Add(
                    $"A marca {brand} nao teve resultados relevantes. Tente aliases, nome oficial, cidade ou categoria."
                );

                AddSourceFailureRecommendation(recommendations, statuses);

                return recommendations;
            }

            var positive =
                CountSentiment(items, "positive");

            var negative =
                CountSentiment(items, "negative");

            if (negative > positive)
            {
                recommendations.Add(
                    $"A marca {brand} tem mais sinais negativos que positivos. Priorize causas, reclamacoes e canais de resposta."
                );
            }
            else if (positive > negative)
            {
                recommendations.Add(
                    $"A marca {brand} tem percepcao favoravel. Reaproveite elogios como prova social e pauta de conteudo."
                );
            }

            var topTerm =
                BuildTopTerms(items, brand).FirstOrDefault();

            if (topTerm is not null)
            {
                recommendations.Add(
                    $"O termo mais associado a {brand} foi #{topTerm.Term}. Vale investigar se e oportunidade ou dor recorrente."
                );
            }

            AddSourceFailureRecommendation(recommendations, statuses);

            return recommendations.Take(4).ToList();
        }

        private static List<string> BuildComparisonRecommendations(
            List<BrandAnalysisDto> brands
        )
        {
            var recommendations =
                new List<string>();

            var orderedByVolume =
                brands
                    .OrderByDescending(brand => brand.TotalMentions)
                    .ToList();

            var leader =
                orderedByVolume.FirstOrDefault();

            if (leader is not null)
            {
                recommendations.Add(
                    $"{leader.Brand} liderou em volume com {leader.TotalMentions} mencoes. Use como referencia competitiva."
                );
            }

            var risk =
                brands
                    .Where(brand => brand.TotalMentions > 0)
                    .OrderByDescending(brand =>
                        (double)brand.Negative / Math.Max(brand.TotalMentions, 1)
                    )
                    .FirstOrDefault();

            if (risk is not null && risk.Negative > 0)
            {
                recommendations.Add(
                    $"{risk.Brand} teve a maior proporcao negativa. Analise os termos associados antes de tirar conclusoes."
                );
            }

            recommendations.Add(
                "Para marcas pequenas, compare tambem por bairro, cidade e categoria; o volume bruto sozinho pode enganar."
            );

            return recommendations;
        }

        public static string BuildTopicCsv(TopicAnalysisResponseDto analysis)
        {
            var builder =
                new StringBuilder();

            builder.AppendLine("source,title,sentiment,engagement,url");

            foreach (var item in analysis.Items)
            {
                builder.AppendLine(ToCsvRow(item));
            }

            return builder.ToString();
        }

        public static string BuildBrandComparisonCsv(
            BrandComparisonResponseDto comparison
        )
        {
            var builder =
                new StringBuilder();

            builder.AppendLine(
                "brand,total_mentions,positive,negative,neutral,engagement"
            );

            foreach (var brand in comparison.Brands)
            {
                builder.AppendLine(
                    string.Join(
                        ",",
                        EscapeCsv(brand.Brand),
                        brand.TotalMentions,
                        brand.Positive,
                        brand.Negative,
                        brand.Neutral,
                        brand.Engagement
                    )
                );
            }

            return builder.ToString();
        }

        private static string ToCsvRow(SocialListeningItemDto item)
        {
            return string.Join(
                ",",
                EscapeCsv(item.Source),
                EscapeCsv(item.Title),
                EscapeCsv(item.Sentiment),
                item.Engagement,
                EscapeCsv(item.Url)
            );
        }

        private static string EscapeCsv(string value)
        {
            var escaped =
                value.Replace("\"", "\"\"");

            return $"\"{escaped}\"";
        }

        private static int CountSentiment(
            IEnumerable<SocialListeningItemDto> items,
            string sentiment
        )
        {
            return items.Count(item => item.Sentiment == sentiment);
        }

        private static bool ContainsTerm(string text, string term)
        {
            return Regex.IsMatch(
                text,
                $@"(^|\W){Regex.Escape(term)}($|\W)",
                RegexOptions.IgnoreCase
            );
        }

        private static IEnumerable<string> ExtractCapitalizedNames(string value)
        {
            return Regex
                .Matches(value, @"\b[A-ZÁÀÂÃÉÈÊÍÓÔÕÚÇ][\wÁÀÂÃÉÈÊÍÓÔÕÚÇáàâãéèêíóôõúç-]{2,}(?:\s+[A-ZÁÀÂÃÉÈÊÍÓÔÕÚÇ][\wÁÀÂÃÉÈÊÍÓÔÕÚÇáàâãéèêíóôõúç-]{2,})?\b")
                .Select(match => match.Value.Trim())
                .Where(name => !StopWords.Contains(Normalize(name)));
        }

        private static IEnumerable<string> SplitBrandInput(string brand)
        {
            return brand.Split(
                ',',
                StringSplitOptions.TrimEntries |
                StringSplitOptions.RemoveEmptyEntries
            );
        }

        private static IEnumerable<string> Tokenize(string value)
        {
            return Regex
                .Matches(value, "[a-z0-9]+")
                .Select(match => match.Value);
        }

        private static string Normalize(string value)
        {
            var normalized =
                value
                    .Trim()
                    .ToLowerInvariant()
                    .Normalize(NormalizationForm.FormD);

            var builder =
                new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (
                    CharUnicodeInfo.GetUnicodeCategory(character) !=
                    UnicodeCategory.NonSpacingMark
                )
                {
                    builder.Append(character);
                }
            }

            return builder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        private static void AddSourceFailureRecommendation(
            List<string> recommendations,
            List<AnalysisSourceStatusDto> statuses
        )
        {
            var failedSources =
                statuses
                    .Where(status => !status.Success)
                    .Select(status => status.Source)
                    .ToList();

            if (failedSources.Count > 0)
            {
                recommendations.Add(
                    $"Revise as fontes que falharam ({string.Join(", ", failedSources)}) antes de usar o resultado em decisao final."
                );
            }
        }

        private record FetchAggregate(
            List<SocialListeningItemDto> Items,
            List<AnalysisSourceStatusDto> Statuses
        );
    }
}
