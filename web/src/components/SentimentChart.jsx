import {
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
} from 'recharts'

function SentimentChart({
  positive,
  negative,
  neutral,
}) {
  const data = [
    {
      name: 'Positivo',
      value: positive,
    },

    {
      name: 'Negativo',
      value: negative,
    },

    {
      name: 'Neutro',
      value: neutral,
    },
  ]

  const total =
    positive + negative + neutral

  const colors = [
    '#16a34a',
    '#dc2626',
    '#71717a',
  ]

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
        justify-between
        gap-4
      ">
        <h2 className="
          text-xl
          font-bold
          text-zinc-950
          dark:text-white
        ">
          Análise de sentimento
        </h2>

        <span className="text-sm text-zinc-500 dark:text-zinc-400">
          {total} itens
        </span>
      </div>

      {total === 0 ? (
        <div className="
          flex
          h-72
          items-center
          justify-center
          rounded-lg
          border
          border-dashed
          border-zinc-300
          text-sm
          text-zinc-500
          dark:border-zinc-700
          dark:text-zinc-400
        ">
          Faça uma busca para gerar o gráfico.
        </div>
      ) : (
        <div className="h-72">
          <ResponsiveContainer>
            <PieChart>
              <Pie
                data={data}
                dataKey="value"
                innerRadius={68}
                outerRadius={112}
                paddingAngle={3}
              >
                {data.map((entry, index) => (
                  <Cell
                    key={entry.name}
                    fill={colors[index]}
                  />
                ))}
              </Pie>

              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </div>
      )}
    </section>
  )
}

export default SentimentChart
