function Trends({ trends, loading }) {
  return (
    <section className="
      rounded-lg
      border
      border-zinc-200
      bg-white
      p-6
      shadow-sm
      dark:border-zinc-800
      dark:bg-zinc-900
    ">
      <h2 className="
        mb-5
        text-xl
        font-bold
        text-zinc-950
        dark:text-white
      ">
        Trends
      </h2>

      {loading && (
        <p className="text-sm text-zinc-500 dark:text-zinc-400">
          Carregando trends...
        </p>
      )}

      {!loading && trends.length === 0 && (
        <p className="text-sm text-zinc-500 dark:text-zinc-400">
          Nenhuma trend encontrada.
        </p>
      )}

      <div className="space-y-3">
        {!loading && trends.map((trend) => (
          <div
            key={trend.keyword}
            className="
              flex
              items-center
              justify-between
              gap-4
              border-b
              border-zinc-100
              pb-3
              last:border-b-0
              last:pb-0
              dark:border-zinc-800
            "
          >
            <span className="
              truncate
              font-semibold
              text-zinc-800
              dark:text-zinc-100
            ">
              #{trend.keyword}
            </span>

            <span className="
              rounded-md
              bg-zinc-950
              px-2.5
              py-1
              text-sm
              font-semibold
              text-white
              dark:bg-white
              dark:text-zinc-950
            ">
              {trend.count}
            </span>
          </div>
        ))}
      </div>
    </section>
  )
}

export default Trends
