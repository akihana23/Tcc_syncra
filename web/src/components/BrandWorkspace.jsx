import { BarChart3, Bookmark, Save, Sparkles, Tags } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'

import api from '../services/api'

const emptyForm = {
  name: '',
  category: '',
  city: '',
  aliases: '',
  competitors: '',
  notes: '',
}

function BrandWorkspace({
  snapshotPayload,
  hasSnapshotData,
}) {
  const [brands, setBrands] = useState([])
  const [form, setForm] = useState(emptyForm)
  const [selectedIds, setSelectedIds] = useState([])
  const [comparison, setComparison] = useState(null)
  const [loading, setLoading] = useState(true)
  const [savingBrand, setSavingBrand] = useState(false)
  const [savingSnapshotId, setSavingSnapshotId] = useState(null)
  const [comparing, setComparing] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    loadBrands()
  }, [])

  const selectedCount =
    selectedIds.length

  const canCompare =
    selectedCount >= 2 && !comparing

  const selectedBrands = useMemo(
    () => brands.filter((brand) => selectedIds.includes(brand.id)),
    [brands, selectedIds]
  )

  async function loadBrands() {
    setLoading(true)

    try {
      const response =
        await api.get('/brands')

      setBrands(response.data ?? [])
    } catch (requestError) {
      console.error(requestError)
      setError('Não foi possível carregar as marcas salvas.')
    } finally {
      setLoading(false)
    }
  }

  function updateField(field, value) {
    setForm((current) => ({
      ...current,
      [field]: value,
    }))
  }

  async function createBrand(event) {
    event.preventDefault()

    if (!form.name.trim() || savingBrand) return

    setSavingBrand(true)
    setMessage('')
    setError('')

    try {
      const response =
        await api.post('/brands', form)

      setBrands((current) => [
        response.data,
        ...current,
      ])
      setForm(emptyForm)
      setMessage('Marca salva. Agora você pode guardar snapshots das buscas.')
    } catch (requestError) {
      console.error(requestError)
      setError('Não foi possível salvar a marca.')
    } finally {
      setSavingBrand(false)
    }
  }

  async function saveSnapshot(brandId) {
    if (!hasSnapshotData || savingSnapshotId) return

    setSavingSnapshotId(brandId)
    setMessage('')
    setError('')

    try {
      await api.post(
        `/brands/${brandId}/snapshots`,
        snapshotPayload
      )

      await loadBrands()

      setMessage('Snapshot salvo para comparação futura.')
    } catch (requestError) {
      console.error(requestError)
      setError('Não foi possível salvar o snapshot da busca atual.')
    } finally {
      setSavingSnapshotId(null)
    }
  }

  function toggleSelected(id) {
    setSelectedIds((current) =>
      current.includes(id)
        ? current.filter((item) => item !== id)
        : [...current, id].slice(0, 5)
    )
  }

  async function compareBrands() {
    if (!canCompare) return

    setComparing(true)
    setMessage('')
    setError('')

    try {
      const response =
        await api.post('/brands/compare', {
          brandIds: selectedIds,
          useAi: true,
        })

      setComparison(response.data)
    } catch (requestError) {
      console.error(requestError)
      setError(
        requestError.response?.data?.message ??
        'Não foi possível comparar as marcas selecionadas.'
      )
    } finally {
      setComparing(false)
    }
  }

  return (
    <div className="space-y-6">
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
        <div className="
          flex
          flex-col
          gap-4
          xl:flex-row
          xl:items-start
          xl:justify-between
        ">
          <div className="max-w-3xl">
            <p className="
              mb-2
              text-xs
              font-semibold
              uppercase
              tracking-wide
              text-zinc-500
              dark:text-zinc-400
            ">
              Base de marcas
            </p>

            <h2 className="
              text-2xl
              font-bold
              text-zinc-950
              dark:text-white
            ">
              Salvar buscas importantes e comparar depois
            </h2>

            <p className="
              mt-3
              text-sm
              leading-6
              text-zinc-600
              dark:text-zinc-300
            ">
              A ideia aqui é transformar uma busca solta em um registro de marca.
              Cada snapshot guarda menções, sentimento, termos, fontes e sugestões,
              permitindo comparar uma marca com concorrentes em momentos diferentes.
            </p>
          </div>

          <div className="
            rounded-lg
            border
            border-zinc-200
            bg-zinc-50
            p-4
            text-sm
            text-zinc-600
            dark:border-zinc-800
            dark:bg-zinc-950
            dark:text-zinc-300
          ">
            {hasSnapshotData
              ? `Busca pronta para salvar: ${snapshotPayload.query}`
              : 'Faça uma busca na barra acima antes de salvar um snapshot.'}
          </div>
        </div>
      </section>

      {(message || error) && (
        <div className={`
          rounded-lg
          border
          px-4
          py-3
          text-sm
          ${
            error
              ? 'border-red-200 bg-red-50 text-red-700 dark:border-red-900 dark:bg-red-950/40 dark:text-red-200'
              : 'border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-900 dark:bg-emerald-950/40 dark:text-emerald-200'
          }
        `}>
          {error || message}
        </div>
      )}

      <div className="
        grid
        grid-cols-1
        gap-6
        xl:grid-cols-[360px_1fr]
      ">
        <BrandForm
          form={form}
          onChange={updateField}
          onSubmit={createBrand}
          saving={savingBrand}
        />

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
          <div className="
            mb-5
            flex
            flex-col
            gap-3
            md:flex-row
            md:items-center
            md:justify-between
          ">
            <div>
              <h2 className="
                text-xl
                font-bold
                text-zinc-950
                dark:text-white
              ">
                Marcas salvas
              </h2>

              <p className="text-sm text-zinc-500 dark:text-zinc-400">
                Selecione duas ou mais marcas com snapshot para comparar.
              </p>
            </div>

            <button
              onClick={compareBrands}
              disabled={!canCompare}
              className="
                inline-flex
                items-center
                justify-center
                gap-2
                rounded-lg
                bg-zinc-950
                px-4
                py-2
                text-sm
                font-semibold
                text-white
                transition
                hover:bg-zinc-800
                disabled:cursor-not-allowed
                disabled:opacity-40
                dark:bg-white
                dark:text-zinc-950
                dark:hover:bg-zinc-200
              "
              type="button"
            >
              <BarChart3 size={18} />
              {comparing
                ? 'Comparando...'
                : `Comparar (${selectedCount})`}
            </button>
          </div>

          {loading && (
            <EmptyState>
              Carregando marcas...
            </EmptyState>
          )}

          {!loading && brands.length === 0 && (
            <EmptyState>
              Nenhuma marca salva ainda.
            </EmptyState>
          )}

          {!loading && brands.length > 0 && (
            <div className="grid gap-4">
              {brands.map((brand) => (
                <BrandCard
                  key={brand.id}
                  brand={brand}
                  selected={selectedIds.includes(brand.id)}
                  onToggle={() => toggleSelected(brand.id)}
                  onSaveSnapshot={() => saveSnapshot(brand.id)}
                  canSaveSnapshot={hasSnapshotData}
                  savingSnapshot={savingSnapshotId === brand.id}
                />
              ))}
            </div>
          )}
        </section>
      </div>

      {selectedBrands.length > 0 && (
        <SelectedPreview brands={selectedBrands} />
      )}

      {comparison && (
        <ComparisonResult comparison={comparison} />
      )}
    </div>
  )
}

function BrandForm({
  form,
  onChange,
  onSubmit,
  saving,
}) {
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
      <div className="
        mb-5
        flex
        items-center
        gap-3
      ">
        <span className="
          inline-flex
          h-10
          w-10
          items-center
          justify-center
          rounded-lg
          bg-sky-50
          text-sky-600
          dark:bg-sky-950
          dark:text-sky-200
        ">
          <Tags size={20} />
        </span>

        <div>
          <h2 className="
            text-xl
            font-bold
            text-zinc-950
            dark:text-white
          ">
            Nova marca
          </h2>

          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            Dados para contextualizar a análise.
          </p>
        </div>
      </div>

      <form
        onSubmit={onSubmit}
        className="space-y-4"
      >
        <Field
          label="Nome da marca"
          value={form.name}
          onChange={(value) => onChange('name', value)}
          required
        />

        <Field
          label="Categoria"
          value={form.category}
          onChange={(value) => onChange('category', value)}
          placeholder="Ex: confeitaria, loja de roupas"
        />

        <Field
          label="Cidade ou região"
          value={form.city}
          onChange={(value) => onChange('city', value)}
        />

        <Field
          label="Aliases"
          value={form.aliases}
          onChange={(value) => onChange('aliases', value)}
          placeholder="Variações do nome, separadas por vírgula"
        />

        <Field
          label="Concorrentes"
          value={form.competitors}
          onChange={(value) => onChange('competitors', value)}
          placeholder="Marcas para comparação futura"
        />

        <label className="block">
          <span className="
            mb-1
            block
            text-sm
            font-medium
            text-zinc-700
            dark:text-zinc-200
          ">
            Observações
          </span>

          <textarea
            value={form.notes}
            onChange={(event) => onChange('notes', event.target.value)}
            rows={3}
            className="
              w-full
              resize-none
              rounded-lg
              border
              border-zinc-200
              bg-zinc-50
              px-3
              py-2
              text-sm
              text-zinc-950
              outline-none
              transition
              focus:border-zinc-500
              dark:border-zinc-800
              dark:bg-zinc-950
              dark:text-white
            "
          />
        </label>

        <button
          disabled={saving || !form.name.trim()}
          className="
            inline-flex
            w-full
            items-center
            justify-center
            gap-2
            rounded-lg
            bg-zinc-950
            px-4
            py-3
            text-sm
            font-semibold
            text-white
            transition
            hover:bg-zinc-800
            disabled:cursor-not-allowed
            disabled:opacity-40
            dark:bg-white
            dark:text-zinc-950
            dark:hover:bg-zinc-200
          "
          type="submit"
        >
          <Bookmark size={18} />
          {saving ? 'Salvando...' : 'Salvar marca'}
        </button>
      </form>
    </section>
  )
}

function Field({
  label,
  value,
  onChange,
  placeholder = '',
  required = false,
}) {
  return (
    <label className="block">
      <span className="
        mb-1
        block
        text-sm
        font-medium
        text-zinc-700
        dark:text-zinc-200
      ">
        {label}
      </span>

      <input
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        required={required}
        className="
          h-11
          w-full
          rounded-lg
          border
          border-zinc-200
          bg-zinc-50
          px-3
          text-sm
          text-zinc-950
          outline-none
          transition
          placeholder:text-zinc-400
          focus:border-zinc-500
          dark:border-zinc-800
          dark:bg-zinc-950
          dark:text-white
        "
      />
    </label>
  )
}

function BrandCard({
  brand,
  selected,
  onToggle,
  onSaveSnapshot,
  canSaveSnapshot,
  savingSnapshot,
}) {
  const latest =
    brand.latestSnapshot

  return (
    <article className="
      rounded-lg
      border
      border-zinc-200
      bg-zinc-50
      p-4
      dark:border-zinc-800
      dark:bg-zinc-950
    ">
      <div className="
        flex
        flex-col
        gap-4
        md:flex-row
        md:items-start
        md:justify-between
      ">
        <label className="
          flex
          min-w-0
          cursor-pointer
          items-start
          gap-3
        ">
          <input
            checked={selected}
            onChange={onToggle}
            className="
              mt-1
              h-4
              w-4
              accent-zinc-950
              dark:accent-white
            "
            type="checkbox"
          />

          <span className="min-w-0">
            <strong className="
              block
              truncate
              text-base
              text-zinc-950
              dark:text-white
            ">
              {brand.name}
            </strong>

            <span className="
              mt-1
              block
              text-sm
              text-zinc-500
              dark:text-zinc-400
            ">
              {[brand.category, brand.city]
                .filter(Boolean)
                .join(' - ') || 'Sem contexto cadastrado'}
            </span>
          </span>
        </label>

        <button
          onClick={onSaveSnapshot}
          disabled={!canSaveSnapshot || savingSnapshot}
          className="
            inline-flex
            shrink-0
            items-center
            justify-center
            gap-2
            rounded-lg
            border
            border-zinc-200
            bg-white
            px-3
            py-2
            text-sm
            font-semibold
            text-zinc-700
            transition
            hover:border-zinc-400
            disabled:cursor-not-allowed
            disabled:opacity-40
            dark:border-zinc-800
            dark:bg-zinc-900
            dark:text-zinc-200
          "
          type="button"
        >
          <Save size={16} />
          {savingSnapshot ? 'Salvando...' : 'Salvar busca atual'}
        </button>
      </div>

      <div className="
        mt-4
        grid
        grid-cols-2
        gap-3
        md:grid-cols-4
      ">
        <MiniMetric
          label="Snapshots"
          value={brand.snapshotCount}
        />

        <MiniMetric
          label="Menções"
          value={latest?.totalMentions ?? 0}
        />

        <MiniMetric
          label="Positivas"
          value={latest?.positive ?? 0}
        />

        <MiniMetric
          label="Negativas"
          value={latest?.negative ?? 0}
        />
      </div>

      {latest && (
        <p className="
          mt-3
          text-xs
          text-zinc-500
          dark:text-zinc-400
        ">
          Último snapshot:
          {' '}
          {formatDate(latest.createdAt)}
          {' '}
          com a busca
          {' '}
          <strong>{latest.query}</strong>
        </p>
      )}
    </article>
  )
}

function MiniMetric({ label, value }) {
  return (
    <div className="
      rounded-lg
      bg-white
      p-3
      dark:bg-zinc-900
    ">
      <span className="block text-xs text-zinc-500 dark:text-zinc-400">
        {label}
      </span>

      <strong className="
        mt-1
        block
        text-lg
        text-zinc-950
        dark:text-white
      ">
        {value}
      </strong>
    </div>
  )
}

function SelectedPreview({ brands }) {
  return (
    <section className="
      rounded-lg
      border
      border-zinc-200
      bg-white
      p-5
      shadow-sm
      dark:border-zinc-800
      dark:bg-zinc-900
    ">
      <h2 className="
        mb-3
        text-lg
        font-bold
        text-zinc-950
        dark:text-white
      ">
        Selecionadas para comparação
      </h2>

      <div className="flex flex-wrap gap-2">
        {brands.map((brand) => (
          <span
            key={brand.id}
            className="
              rounded-md
              bg-zinc-100
              px-3
              py-2
              text-sm
              font-medium
              text-zinc-700
              dark:bg-zinc-950
              dark:text-zinc-200
            "
          >
            {brand.name}
          </span>
        ))}
      </div>
    </section>
  )
}

function ComparisonResult({ comparison }) {
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
      <div className="
        mb-5
        flex
        items-center
        gap-3
      ">
        <span className="
          inline-flex
          h-10
          w-10
          items-center
          justify-center
          rounded-lg
          bg-rose-50
          text-rose-600
          dark:bg-rose-950
          dark:text-rose-200
        ">
          <Sparkles size={20} />
        </span>

        <div>
          <h2 className="
            text-xl
            font-bold
            text-zinc-950
            dark:text-white
          ">
            Comparação salva
          </h2>

          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            {comparison.aiStatus}
          </p>
        </div>
      </div>

      <div className="
        mb-6
        grid
        grid-cols-1
        gap-4
        md:grid-cols-2
        xl:grid-cols-3
      ">
        {comparison.brands.map((item) => (
          <article
            key={item.brand.id}
            className="
              rounded-lg
              bg-zinc-50
              p-4
              dark:bg-zinc-950
            "
          >
            <h3 className="
              text-lg
              font-bold
              text-zinc-950
              dark:text-white
            ">
              {item.brand.name}
            </h3>

            <div className="
              mt-4
              grid
              grid-cols-3
              gap-2
              text-center
            ">
              <MiniMetric label="Menções" value={item.snapshot.totalMentions} />
              <MiniMetric label="+" value={item.snapshot.positive} />
              <MiniMetric label="-" value={item.snapshot.negative} />
            </div>
          </article>
        ))}
      </div>

      <InsightList
        title="Leitura automática"
        items={comparison.recommendations}
      />

      {comparison.aiInsights.length > 0 && (
        <InsightList
          title="Insights por IA"
          items={comparison.aiInsights}
        />
      )}
    </section>
  )
}

function InsightList({ title, items }) {
  return (
    <div className="mt-5 first:mt-0">
      <h3 className="
        mb-3
        text-base
        font-bold
        text-zinc-950
        dark:text-white
      ">
        {title}
      </h3>

      <div className="grid gap-3">
        {items.map((item) => (
          <p
            key={item}
            className="
              rounded-lg
              bg-zinc-50
              p-3
              text-sm
              leading-6
              text-zinc-700
              dark:bg-zinc-950
              dark:text-zinc-200
            "
          >
            {item}
          </p>
        ))}
      </div>
    </div>
  )
}

function EmptyState({ children }) {
  return (
    <div className="
      rounded-lg
      border
      border-dashed
      border-zinc-300
      p-8
      text-center
      text-sm
      text-zinc-500
      dark:border-zinc-700
      dark:text-zinc-400
    ">
      {children}
    </div>
  )
}

function formatDate(value) {
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value))
}

export default BrandWorkspace
