function InsightCard({
  title,
  description,
}) {
  return (
    <div className="
      bg-white
      dark:bg-zinc-900
      rounded-2xl
      shadow
      p-6
    ">
      <h3 className="
        text-xl
        font-bold
        mb-3
        dark:text-white
      ">
        {title}
      </h3>

      <p className="
        text-gray-600
        dark:text-gray-300
      ">
        {description}
      </p>
    </div>
  )
}

export default InsightCard