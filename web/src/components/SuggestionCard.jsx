import { Sparkles } from 'lucide-react'

function SuggestionCard({
  suggestions,
  sourceLabel,
  loading,
}) {
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
      <div className="
        mb-5
        flex
        items-center
        gap-3
      ">
        <span className="
          inline-flex
          h-10
          w-10
          items-center
          justify-center
          rounded-lg
          bg-rose-50
          text-rose-600
          dark:bg-rose-950
          dark:text-rose-200
        ">
          <Sparkles size={20} />
        </span>

        <div>
          <h2 className="
            text-xl
            font-bold
            text-zinc-950
            dark:text-white
          ">
            Sugestões rápidas
          </h2>

          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            {loading ? 'Gerando recomendações...' : sourceLabel}
          </p>
        </div>
      </div>

      <div className="space-y-3">
        {loading && (
          <div className="
            rounded-lg
            bg-zinc-50
            p-3
            text-sm
            leading-6
            text-zinc-500
            dark:bg-zinc-950
            dark:text-zinc-400
          ">
            Analisando resultados...
          </div>
        )}

        {!loading && suggestions.map((suggestion) => (
          <div
            key={suggestion}
            className="
              rounded-lg
              bg-zinc-50
              p-3
              text-sm
              leading-6
              text-zinc-700
              dark:bg-zinc-950
              dark:text-zinc-200
            "
          >
            {suggestion}
          </div>
        ))}
      </div>
    </section>
  )
}

export default SuggestionCard
