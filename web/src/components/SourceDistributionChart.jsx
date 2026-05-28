import {
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
} from 'recharts'

const COLORS = ['#f97316', '#dc2626', '#0284c7']

function SourceDistributionChart({ data }) {
  const chartData = data.filter((item) => item.mentions > 0)

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
        Volume por fonte
      </h2>

      {chartData.length === 0 ? (
        <EmptyState />
      ) : (
        <>
          <div className="h-56">
            <ResponsiveContainer>
              <PieChart>
                <Pie
                  data={chartData}
                  dataKey="mentions"
                  nameKey="name"
                  innerRadius={58}
                  outerRadius={92}
                  paddingAngle={4}
                >
                  {chartData.map((entry, index) => (
                    <Cell
                      key={entry.name}
                      fill={COLORS[index % COLORS.length]}
                    />
                  ))}
                </Pie>

                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </div>

          <div className="mt-3 space-y-2">
            {chartData.map((item, index) => (
              <div
                key={item.name}
                className="
                  flex
                  items-center
                  justify-between
                  text-sm
                  text-zinc-600
                  dark:text-zinc-300
                "
              >
                <span className="flex items-center gap-2">
                  <span
                    className="h-2.5 w-2.5 rounded-full"
                    style={{
                      backgroundColor: COLORS[index % COLORS.length],
                    }}
                  />
                  {item.name}
                </span>

                <strong className="text-zinc-950 dark:text-white">
                  {item.mentions}
                </strong>
              </div>
            ))}
          </div>
        </>
      )}
    </section>
  )
}

function EmptyState() {
  return (
    <div className="
      flex
      h-56
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
      Nenhuma fonte carregada ainda.
    </div>
  )
}

export default SourceDistributionChart
