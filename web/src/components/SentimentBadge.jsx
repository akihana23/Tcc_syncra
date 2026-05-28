function SentimentBadge({ sentiment }) {
  const styles = {
    positive:
      'bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-200',

    negative:
      'bg-red-100 text-red-800 dark:bg-red-950 dark:text-red-200',

    neutral:
      'bg-zinc-200 text-zinc-700 dark:bg-zinc-800 dark:text-zinc-200',
  }

  const labels = {
    positive: 'Positivo',

    negative: 'Negativo',

    neutral: 'Neutro',
  }

  const safeSentiment =
    styles[sentiment] ? sentiment : 'neutral'

  return (
    <span
      className={`
        shrink-0
        rounded-md
        px-2
        py-1
        text-xs
        font-bold
        ${styles[safeSentiment]}
      `}
    >
      {labels[safeSentiment]}
    </span>
  )
}

export default SentimentBadge
