function MetricCard({ title, value, growth }) {
  return (
    <section className="
      rounded-lg
      border
      border-zinc-200
      bg-white
      p-5
      shadow-sm
      transition
      dark:border-zinc-800
      dark:bg-zinc-900
    ">
      <p className="
        mb-3
        text-sm
        font-medium
        text-zinc-500
        dark:text-zinc-400
      ">
        {title}
      </p>

      <div className="
        flex
        items-end
        justify-between
        gap-4
      ">
        <h3 className="
          text-3xl
          font-bold
          text-zinc-950
          dark:text-white
        ">
          {value}
        </h3>

        <span className="
          rounded-md
          bg-emerald-50
          px-2
          py-1
          text-xs
          font-semibold
          text-emerald-700
          dark:bg-emerald-950
          dark:text-emerald-300
        ">
          {growth}
        </span>
      </div>
    </section>
  )
}

export default MetricCard
