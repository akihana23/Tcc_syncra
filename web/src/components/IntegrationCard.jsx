function IntegrationCard({
  name,
  status,
  description,
  tone = 'ready',
}) {
  const tones = {
    ready:
      'bg-emerald-50 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-200',
    partial:
      'bg-amber-50 text-amber-800 dark:bg-amber-950 dark:text-amber-200',
    blocked:
      'bg-red-50 text-red-700 dark:bg-red-950 dark:text-red-200',
  }

  return (
    <article className="
      rounded-lg
      border
      border-zinc-200
      bg-white
      p-5
      shadow-sm
      dark:border-zinc-800
      dark:bg-zinc-900
    ">
      <div className="
        mb-3
        flex
        items-start
        justify-between
        gap-3
      ">
        <h3 className="
          text-lg
          font-bold
          text-zinc-950
          dark:text-white
        ">
          {name}
        </h3>

        <span className={`
          rounded-md
          px-2
          py-1
          text-xs
          font-bold
          ${tones[tone]}
        `}>
          {status}
        </span>
      </div>

      <p className="
        text-sm
        leading-6
        text-zinc-600
        dark:text-zinc-300
      ">
        {description}
      </p>
    </article>
  )
}

export default IntegrationCard
