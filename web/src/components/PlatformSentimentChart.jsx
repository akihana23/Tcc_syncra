import {
  Bar,
  BarChart,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'

function PlatformSentimentChart({ data }) {
  const hasData = data.some((item) => item.mentions > 0)

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
        Sentimento por plataforma
      </h2>

      {!hasData ? (
        <EmptyState />
      ) : (
        <ResponsiveContainer width="100%" height={280}>
          <BarChart data={data}>
            <XAxis dataKey="name" tickLine={false} />
            <YAxis allowDecimals={false} />
            <Tooltip />
            <Legend />
            <Bar
              dataKey="positive"
              name="Positivo"
              stackId="sentiment"
              fill="#16a34a"
              radius={[4, 4, 0, 0]}
            />
            <Bar
              dataKey="neutral"
              name="Neutro"
              stackId="sentiment"
              fill="#71717a"
            />
            <Bar
              dataKey="negative"
              name="Negativo"
              stackId="sentiment"
              fill="#dc2626"
            />
          </BarChart>
        </ResponsiveContainer>
      )}
    </section>
  )
}

function EmptyState() {
  return (
    <div className="
      flex
      h-64
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
      Busque uma marca para comparar sentimento.
    </div>
  )
}

export default PlatformSentimentChart
