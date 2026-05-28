import Sidebar from '../components/Sidebar'
import Header from '../components/Header'

function DashboardLayout({
  children,
  darkMode,
  setDarkMode,
  activeView,
  onViewChange,
  viewTitle,
}) {
  return (
    <div className="
      min-h-screen
      bg-zinc-100
      text-zinc-950
      transition
      dark:bg-zinc-950
      dark:text-white
      lg:flex
    ">
      <Sidebar
        activeView={activeView}
        onViewChange={onViewChange}
      />

      <div className="min-w-0 flex-1">
        <Header
          darkMode={darkMode}
          setDarkMode={setDarkMode}
          viewTitle={viewTitle}
        />

        <main className="
          mx-auto
          max-w-7xl
          p-4
          md:p-6
          xl:p-8
        ">
          {children}
        </main>
      </div>
    </div>
  )
}

export default DashboardLayout
