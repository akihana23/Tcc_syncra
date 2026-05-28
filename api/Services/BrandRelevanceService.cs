using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SocialListening.API.Services
{
    public class BrandRelevanceService
    {
        private static readonly HashSet<string> StopWords = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "a", "as", "o", "os", "um", "uma", "uns", "umas",
            "de", "da", "do", "das", "dos", "em", "no", "na",
            "nos", "nas", "e", "ou", "com", "para", "por",
            "sobre", "marca", "empresa", "produto", "servico",
            "serviço", "loja", "oficial", "brasil"
        };

        private static readonly Dictionary<string, string[]> KnownContexts = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["tim"] =
            [
                "tim brasil", "tim live", "tim controle", "tim beta",
                "tim black", "tim ultra", "meu tim", "timbrasil",
                "telefonia", "operadora", "celular", "internet",
                "fibra", "sinal", "plano", "planos", "chip",
                "recarga", "fatura", "conta", "5g", "4g", "dados",
                "roaming", "portabilidade", "cobertura", "ligacao",
                "ligacoes", "telefone", "protocolo", "atendimento"
            ],
            ["vivo"] =
            [
                "vivo brasil", "vivo fibra", "meu vivo", "telefonia",
                "operadora", "celular", "internet", "fibra", "sinal",
                "plano", "chip", "fatura", "5g", "4g", "cobertura"
            ],
            ["claro"] =
            [
                "claro brasil", "claro net", "minha claro", "telefonia",
                "operadora", "celular", "internet", "fibra", "sinal",
                "plano", "chip", "fatura", "5g", "4g", "cobertura"
            ],
            ["oi"] =
            [
                "oi fibra", "minha oi", "telefonia", "operadora",
                "celular", "internet", "fibra", "sinal", "plano",
                "chip", "fatura", "cobertura"
            ],
        };

        private static readonly Dictionary<string, string[]> KnownAliases = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["tim"] = ["timbrasil", "meu tim"],
            ["vivo"] = ["vivobrasil", "meu vivo"],
            ["claro"] = ["clarobrasil", "minha claro"],
            ["oi"] = ["oi fibra", "minha oi"],
        };

        public string BuildExternalQuery(string query)
        {
            var tokens =
                GetMeaningfulTokens(query);

            if (
                tokens.Count == 1 &&
                KnownContexts.TryGetValue(tokens[0], out var contexts)
            )
            {
                return $"{query.Trim()} {string.Join(' ', contexts.Take(4))}";
            }

            return query.Trim();
        }

        public List<T> FilterRelevant<T>(
            IEnumerable<T> items,
            string query,
            Func<T, string> textSelector,
            int maxResults = 10
        )
        {
            var list =
                items.ToList();

            var relevant =
                list
                    .Where(item => IsRelevant(query, textSelector(item)))
                    .Take(maxResults)
                    .ToList();

            if (ShouldRequireRelevance(query))
            {
                return relevant;
            }

            return relevant.Count > 0
                ? relevant
                : list.Take(maxResults).ToList();
        }

        private static bool IsRelevant(string query, string text)
        {
            var queryTokens =
                GetMeaningfulTokens(query);

            if (queryTokens.Count == 0)
            {
                return true;
            }

            var normalizedText =
                Normalize(text);

            var textTokens =
                Tokenize(normalizedText).ToHashSet(
                    StringComparer.OrdinalIgnoreCase
                );

            if (queryTokens.Count > 1)
            {
                var normalizedQuery =
                    Normalize(query);

                if (normalizedText.Contains(normalizedQuery))
                {
                    return true;
                }

                var ambiguousToken =
                    queryTokens.FirstOrDefault(IsAmbiguousToken);

                if (ambiguousToken is not null)
                {
                    return
                        HasBrandSignal(ambiguousToken, normalizedText, textTokens) &&
                        (
                            queryTokens
                                .Where(token => token != ambiguousToken)
                                .Any(textTokens.Contains) ||
                            HasKnownContext(ambiguousToken, normalizedText)
                        );
                }

                return queryTokens.All(textTokens.Contains);
            }

            var token =
                queryTokens[0];

            if (!HasBrandSignal(token, normalizedText, textTokens))
            {
                return false;
            }

            if (KnownContexts.ContainsKey(token))
            {
                return HasKnownContext(token, normalizedText);
            }

            return true;
        }

        private static bool ShouldRequireRelevance(string query)
        {
            var tokens =
                GetMeaningfulTokens(query);

            return tokens.Any(IsAmbiguousToken);
        }

        private static bool IsAmbiguousToken(string token)
        {
            return
                token.Length <= 3 ||
                KnownContexts.ContainsKey(token);
        }

        private static bool HasBrandSignal(
            string token,
            string normalizedText,
            HashSet<string> textTokens
        )
        {
            if (textTokens.Contains(token))
            {
                return true;
            }

            return
                KnownAliases.TryGetValue(token, out var aliases) &&
                aliases.Any(alias => normalizedText.Contains(Normalize(alias)));
        }

        private static bool HasKnownContext(
            string token,
            string normalizedText
        )
        {
            return
                KnownContexts.TryGetValue(token, out var contexts) &&
                contexts.Any(context =>
                    normalizedText.Contains(Normalize(context))
                );
        }

        private static List<string> GetMeaningfulTokens(string value)
        {
            return Tokenize(Normalize(value))
                .Where(token => !StopWords.Contains(token))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
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
    }
}
