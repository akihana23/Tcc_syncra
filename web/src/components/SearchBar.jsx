import { Search } from 'lucide-react'

function SearchBar({
  search,
  setSearch,
  handleSearch,
  loading,
}) {
  return (
    <section className="
      mb-6
      rounded-lg
      border
      border-zinc-200
      bg-white
      p-4
      shadow-sm
      dark:border-zinc-800
      dark:bg-zinc-900
    ">
      <form
        onSubmit={handleSearch}
        className="
          flex
          flex-col
          gap-3
          md:flex-row
        "
      >
        <div className="relative flex-1">
          <Search
            size={18}
            className="
              absolute
              left-4
              top-1/2
              -translate-y-1/2
              text-zinc-400
            "
          />

          <input
            type="text"
            placeholder="Pesquisar marca, produto ou concorrente"
            value={search}
            disabled={loading}
            onChange={(e) =>
              setSearch(e.target.value)
            }
            className="
              h-12
              w-full
              rounded-lg
              border
              border-zinc-200
              bg-zinc-50
              pl-11
              pr-4
              text-zinc-950
              outline-none
              transition
              placeholder:text-zinc-400
              focus:border-zinc-500
              disabled:opacity-50
              dark:border-zinc-800
              dark:bg-zinc-950
              dark:text-white
            "
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="
            h-12
            rounded-lg
            bg-zinc-950
            px-5
            font-semibold
            text-white
            transition
            hover:bg-zinc-800
            disabled:cursor-not-allowed
            disabled:opacity-50
            dark:bg-white
            dark:text-zinc-950
            dark:hover:bg-zinc-200
          "
        >
          {loading
            ? 'Pesquisando...'
            : 'Pesquisar'}
        </button>
      </form>
    </section>
  )
}

export default SearchBar
