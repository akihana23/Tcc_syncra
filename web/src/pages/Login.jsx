import { useContext, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import api from '../services/api'

import { AuthContext } from '../contexts/AuthContextValue'

function Login() {
  const navigate = useNavigate()

  const { login } = useContext(AuthContext)

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleLogin(e) {
    e.preventDefault()

    setError('')
    setLoading(true)

    try {
      const response = await api.post('/auth/login', {
        email,
        password,
      })

      login(response.data.token)

      navigate('/dashboard')

    } catch (requestError) {
      console.error(requestError)

      setError('Email ou senha inválidos.')

    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="
      flex
      min-h-screen
      items-center
      justify-center
      bg-zinc-100
      px-4
      text-zinc-950
      dark:bg-zinc-950
      dark:text-white
    ">
      <form
        onSubmit={handleLogin}
        className="
          w-full
          max-w-sm
          rounded-lg
          border
          border-zinc-200
          bg-white
          p-6
          shadow-sm
          dark:border-zinc-800
          dark:bg-zinc-900
        "
      >
        <div className="
          mb-6
          flex
          items-center
          gap-3
        ">
          <img
            src="/syncra-logo.jpeg"
            alt="Syncra"
            className="
              h-12
              w-12
              rounded-lg
              object-cover
            "
          />

          <div>
            <p className="
              text-xs
              font-semibold
              uppercase
              tracking-wide
              text-zinc-500
              dark:text-zinc-400
            ">
              Syncra
            </p>

            <p className="text-sm text-zinc-500 dark:text-zinc-400">
              Social listening acessível
            </p>
          </div>
        </div>

        <h1 className="
          mb-6
          text-3xl
          font-bold
        ">
          Entrar
        </h1>

        {error && (
          <div className="
            mb-4
            rounded-lg
            border
            border-red-200
            bg-red-50
            px-3
            py-2
            text-sm
            text-red-700
            dark:border-red-900
            dark:bg-red-950/40
            dark:text-red-200
          ">
            {error}
          </div>
        )}

        <label className="mb-4 block">
          <span className="mb-2 block text-sm font-medium">
            Email
          </span>

          <input
            type="email"
            value={email}
            className="
              h-12
              w-full
              rounded-lg
              border
              border-zinc-200
              bg-zinc-50
              px-4
              outline-none
              transition
              focus:border-zinc-500
              dark:border-zinc-800
              dark:bg-zinc-950
            "
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </label>

        <label className="mb-6 block">
          <span className="mb-2 block text-sm font-medium">
            Senha
          </span>

          <input
            type="password"
            value={password}
            className="
              h-12
              w-full
              rounded-lg
              border
              border-zinc-200
              bg-zinc-50
              px-4
              outline-none
              transition
              focus:border-zinc-500
              dark:border-zinc-800
              dark:bg-zinc-950
            "
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </label>

        <button
          type="submit"
          disabled={loading}
          className="
            h-12
            w-full
            rounded-lg
            bg-zinc-950
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
          {loading ? 'Entrando...' : 'Entrar'}
        </button>
      </form>
    </main>
  )
}

export default Login
