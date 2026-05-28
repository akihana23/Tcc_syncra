import { Moon, Sun } from 'lucide-react'

function Header({ darkMode, setDarkMode, viewTitle }) {
  return (
    <header className="
      sticky
      top-0
      z-10
      border-b
      border-zinc-200
      bg-white/90
      px-4
      py-4
      backdrop-blur
      dark:border-zinc-800
      dark:bg-zinc-900/90
      md:px-6
      xl:px-8
    ">
      <div className="
        mx-auto
        flex
        max-w-7xl
        items-center
        justify-between
        gap-4
      ">
        <div>
          <h2 className="
            text-lg
            font-bold
            text-zinc-950
            dark:text-white
          ">
            {viewTitle}
          </h2>

          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            Syncra monitora conversas para pequenos negócios.
          </p>
        </div>

        <button
          onClick={() => setDarkMode(!darkMode)}
          className="
            inline-flex
            h-10
            w-10
            items-center
            justify-center
            rounded-lg
            bg-zinc-100
            text-zinc-700
            transition
            hover:bg-zinc-200
            dark:bg-zinc-800
            dark:text-white
            dark:hover:bg-zinc-700
          "
          title={darkMode ? 'Ativar tema claro' : 'Ativar tema escuro'}
          type="button"
        >
          {darkMode ? <Sun size={20} /> : <Moon size={20} />}
        </button>
      </div>
    </header>
  )
}

export default Header
