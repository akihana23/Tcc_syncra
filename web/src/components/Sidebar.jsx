import {
  BarChart3,
  Bookmark,
  LayoutDashboard,
  LogOut,
  MessageCircle,
  ClipboardList,
} from 'lucide-react'

import { useContext } from 'react'
import { AuthContext } from '../contexts/AuthContextValue'

const navItems = [
  {
    id: 'overview',
    label: 'Visão geral',
    icon: LayoutDashboard,
  },
  {
    id: 'analytics',
    label: 'Analytics',
    icon: BarChart3,
  },
  {
    id: 'mentions',
    label: 'Menções',
    icon: MessageCircle,
  },
  {
    id: 'brands',
    label: 'Marcas',
    icon: Bookmark,
  },
  {
    id: 'settings',
    label: 'TCC',
    icon: ClipboardList,
  },
]

function Sidebar({ activeView, onViewChange }) {
  const { logout } = useContext(AuthContext)

  return (
    <aside className="
      border-b
      border-zinc-800
      bg-zinc-950
      p-4
      text-white
      lg:sticky
      lg:top-0
      lg:h-screen
      lg:w-64
      lg:border-b-0
      lg:p-6
    ">
      <div className="
        flex
        items-center
        justify-between
        gap-4
        lg:block
      ">
        <div className="
          flex
          items-center
          gap-3
          lg:mb-10
        ">
          <img
            src="/syncra-logo.jpeg"
            alt="Syncra"
            className="
              h-11
              w-11
              rounded-lg
              object-cover
            "
          />

          <div>
            <h1 className="text-xl font-bold">
              Syncra
            </h1>

            <p className="text-xs text-zinc-400">
              Social listening
            </p>
          </div>
        </div>

        <button
          onClick={logout}
          className="
            inline-flex
            items-center
            gap-2
            rounded-lg
            px-3
            py-2
            text-sm
            text-red-300
            transition
            hover:bg-red-950
            hover:text-red-200
            lg:hidden
          "
          type="button"
        >
          <LogOut size={18} />
          Sair
        </button>
      </div>

      <nav className="
        mt-4
        flex
        gap-2
        overflow-x-auto
        lg:mt-0
        lg:flex-col
        lg:gap-3
        lg:overflow-visible
      ">
        {navItems.map((item) => (
          <SidebarButton
            key={item.id}
            active={activeView === item.id}
            icon={<item.icon size={20} />}
            onClick={() => onViewChange(item.id)}
          >
            {item.label}
          </SidebarButton>
        ))}
      </nav>

      <button
        onClick={logout}
        className="
          mt-10
          hidden
          w-full
          items-center
          gap-3
          rounded-lg
          px-3
          py-2
          text-left
          text-red-300
          transition
          hover:bg-red-950
          hover:text-red-200
          lg:flex
        "
        type="button"
      >
        <LogOut size={20} />
        Logout
      </button>
    </aside>
  )
}

function SidebarButton({
  active = false,
  icon,
  children,
  onClick,
}) {
  return (
    <button
      onClick={onClick}
      className={`
        inline-flex
        shrink-0
        items-center
        gap-3
        rounded-lg
        px-3
        py-2
        text-sm
        font-medium
        transition
        lg:w-full

        ${
          active
            ? 'bg-white text-zinc-950'
            : 'text-zinc-300 hover:bg-zinc-900 hover:text-white'
        }
      `}
      type="button"
    >
      {icon}
      {children}
    </button>
  )
}

export default Sidebar
