namespace SocialListening.API.Services
{
    public class SentimentService
    {
        private readonly List<string> PositiveWords =
        [
            "bom",
            "ótimo",
            "excelente",
            "incrível",
            "recomendo",
            "maravilhoso",
            "amei",
            "top",
            "perfeito",
            "gostei",
            "love",
            "great",
            "amazing",
            "best",
            "fantastic",
            "awesome",
            "wonderful",
            "happily",
        ];

        private readonly List<string> NegativeWords =
        [
            "ruim",
            "péssimo",
            "horrível",
            "lixo",
            "terrível",
            "odiei",
            "pior",
            "never",
            "bad",
            "worst",
            "hate",
            "problem",
            "evil",
            "awful",
            "disappointing",
            "terrible",
            "hateful"

        ];

        public string Analyze(string text)
        {
            text = text.ToLowerInvariant();

            int positiveScore = 0;

            int negativeScore = 0;

            foreach (var word in PositiveWords)
            {
                if (text.Contains(word))
                {
                    positiveScore++;
                }
            }

            foreach (var word in NegativeWords)
            {
                if (text.Contains(word))
                {
                    negativeScore++;
                }
            }

            if (positiveScore > negativeScore)
            {
                return "positive";
            }

            if (negativeScore > positiveScore)
            {
                return "negative";
            }

            return "neutral";
        }
    }
}
