const STOP_WORDS = new Set([
  'the',
  'and',
  'for',
  'with',
  'this',
  'that',
  'from',
  'como',
  'para',
  'sobre',
  'voce',
  'porque',
  'quando',
  'esta',
  'esse',
  'essa',
  'isso',
  'mais',
  'muito',
  'todo',
  'toda',
  'de',
  'da',
  'do',
  'das',
  'dos',
  'em',
  'uma',
  'com',
])

export function buildTrends(items, limit = 10) {
  const counts = new Map()

  items.forEach((item) => {
    const words =
      item.title
        ?.normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .toLowerCase()
        .match(/[a-z0-9]+/g) ?? []

    words
      .filter((word) => word.length > 3)
      .filter((word) => !STOP_WORDS.has(word))
      .forEach((word) => {
        counts.set(word, (counts.get(word) ?? 0) + 1)
      })
  })

  return [...counts.entries()]
    .map(([keyword, count]) => ({
      keyword,
      count,
    }))
    .sort((a, b) => b.count - a.count)
    .slice(0, limit)
}

export function countSentiment(items) {
  return {
    positive: items.filter((item) => item.sentiment === 'positive').length,
    negative: items.filter((item) => item.sentiment === 'negative').length,
    neutral: items.filter((item) => item.sentiment === 'neutral').length,
  }
}

export function buildPlatformData(mastodonPosts, videos, blueskyPosts = []) {
  const mastodonSentiment = countSentiment(mastodonPosts)
  const youtubeSentiment = countSentiment(videos)
  const blueskySentiment = countSentiment(blueskyPosts)

  return [
    {
      name: 'Mastodon',
      mentions: mastodonPosts.length,
      engagement: mastodonPosts.reduce(
        (acc, post) =>
          acc +
          Number(post.favourites ?? 0) +
          Number(post.boosts ?? 0) +
          Number(post.replies ?? 0),
        0
      ),
      ...mastodonSentiment,
    },
    {
      name: 'YouTube',
      mentions: videos.length,
      engagement: videos.length,
      ...youtubeSentiment,
    },
    {
      name: 'BlueSky',
      mentions: blueskyPosts.length,
      engagement: blueskyPosts.reduce(
        (acc, post) => acc + post.likes + post.replies + post.reposts,
        0
      ),
      ...blueskySentiment,
    },
  ]
}

export function buildSuggestions({
  totalMentions,
  positivePercentage,
  negativeCount,
  topTrend,
  mostNegativePlatform,
  sourceErrors,
}) {
  if (totalMentions === 0) {
    return [
      'Faça uma busca pelo nome da empresa, produto principal ou concorrente direto.',
      'Use termos simples primeiro e refine com palavras como bairro, cidade ou categoria.',
    ]
  }

  const suggestions = []

  if (positivePercentage >= 60) {
    suggestions.push(
      'A percepção está favorável. Aproveite os termos positivos em posts, anúncios e respostas públicas.'
    )
  }

  if (negativeCount > 0) {
    suggestions.push(
      `Existe ruído negativo em ${mostNegativePlatform}. Priorize respostas objetivas e registre temas recorrentes.`
    )
  }

  if (topTrend) {
    suggestions.push(
      `O termo #${topTrend.keyword} apareceu com destaque. Ele pode virar pauta para conteúdo ou melhoria operacional.`
    )
  }

  if (sourceErrors.length > 0) {
    suggestions.push(
      `Algumas fontes falharam nesta busca: ${sourceErrors.join(' e ')}. Refaça a consulta antes de tomar decisão.`
    )
  }

  return suggestions.slice(0, 4)
}
