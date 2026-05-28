import {
  Bar,
  BarChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'

function EngagementSummaryChart({ data }) {
  const hasData = data.some((item) => item.engagement > 0)

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
        items-start
        justify-between
        gap-4
      ">
        <div>
          <h2 className="
            text-xl
            font-bold
            text-zinc-950
            dark:text-white
          ">
            Engajamento estimado
          </h2>

          <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
            Reddit usa upvotes + comentários; YouTube usa volume enquanto a API não traz métricas.
          </p>
        </div>
      </div>

      {!hasData ? (
        <EmptyState />
      ) : (
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={data}>
            <XAxis dataKey="name" tickLine={false} />
            <YAxis allowDecimals={false} />
            <Tooltip />
            <Bar
              dataKey="engagement"
              name="Engajamento"
              fill="#2563eb"
              radius={[6, 6, 0, 0]}
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
      h-60
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
      Nenhum engajamento calculado ainda.
    </div>
  )
}

export default EngagementSummaryChart
