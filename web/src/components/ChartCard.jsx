import {
  Bar,
  BarChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
} from 'recharts'

function ChartCard({ posts }) {
  const data = posts
    .slice(0, 5)
    .map((post) => ({
      name: post.subreddit,
      mentions: post.score,
    }))

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
        Subreddits por engajamento
      </h2>

      {data.length === 0 ? (
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
          Nenhum dado de Reddit ainda.
        </div>
      ) : (
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={data}>
            <XAxis
              dataKey="name"
              tickLine={false}
            />

            <Tooltip />

            <Bar
              dataKey="mentions"
              fill="#f97316"
              radius={[6, 6, 0, 0]}
            />
          </BarChart>
        </ResponsiveContainer>
      )}
    </section>
  )
}

export default ChartCard
