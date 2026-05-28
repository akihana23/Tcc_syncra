import { useEffect, useState } from 'react'

import { AuthProvider } from './contexts/AuthContext'

import AppRoutes from './routes/AppRoutes'

function App() {
  const [darkMode, setDarkMode] = useState(() => {
    return localStorage.getItem('theme') === 'dark'
  })

  useEffect(() => {
    if (darkMode) {
      document.documentElement.classList.add('dark')

      localStorage.setItem('theme', 'dark')
    } else {
      document.documentElement.classList.remove('dark')

      localStorage.setItem('theme', 'light')
    }
  }, [darkMode])

  return (
    <AuthProvider>
      <AppRoutes
        darkMode={darkMode}
        setDarkMode={setDarkMode}
      />
    </AuthProvider>
  )
}

export default App