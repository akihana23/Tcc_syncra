import { CheckCircle2, Clock3, Compass, Layers3 } from 'lucide-react'

const completedItems = [
  'Criamos autenticação básica, dashboard e histórico de buscas.',
  'Integramos Reddit, YouTube e BlueSky como primeiras fontes públicas.',
  'Montamos gráficos de sentimento, volume, engajamento e termos recorrentes.',
  'Preparamos a API para salvar marcas, guardar snapshots e comparar resultados.',
  'Deixamos a IA no backend para gerar sugestões sem expor a chave no frontend.',
]

const todoItems = [
  {
    status: 'Próximo',
    title: 'Validar com casos reais',
    description:
      'Vamos testar marcas pequenas ou MEIs conhecidos para avaliar se os insights ajudam em decisões simples de comunicação.',
  },
  {
    status: 'Próximo',
    title: 'Melhorar filtros de contexto',
    description:
      'Queremos reduzir falsos positivos em nomes curtos, marcas ambíguas e buscas muito genéricas.',
  },
  {
    status: 'Próximo',
    title: 'Publicar online',
    description:
      'Vamos separar variáveis de ambiente, apontar banco Neon, subir a API no Render e o frontend no Vercel.',
  },
  {
    status: 'Futuro',
    title: 'Relatório apresentável',
    description:
      'Pretendemos gerar um PDF ou página compartilhável com resumo, gráficos, limitações e recomendações.',
  },
]

const sources = [
  {
    name: 'Reddit',
    status: 'Feito',
    description:
      'Escolhemos o Reddit porque muitas pessoas relatam experiências, dúvidas e reclamações espontâneas. Para pequenas empresas, isso ajuda a captar dores e linguagem real do público.',
  },
  {
    name: 'YouTube',
    status: 'Feito',
    description:
      'Escolhemos o YouTube porque vídeos e comentários são bons sinais de interesse, reputação e temas associados a produtos, marcas e categorias.',
  },
  {
    name: 'BlueSky',
    status: 'Feito',
    description:
      'Escolhemos o BlueSky por ter busca pública e acesso mais simples para acompanhar conversas abertas e temas emergentes sem depender de aprovação complexa.',
  },
  {
    name: 'Instagram',
    status: 'Futuro',
    description:
      'Deixamos para uma etapa futura porque a API da Meta exige conta profissional, app configurado, permissões e tokens. Ela faz sentido para uma versão mais madura.',
  },
  {
    name: 'TikTok',
    status: 'Futuro',
    description:
      'Deixamos como futura inserção porque o acesso oficial tende a ser mais restrito. Para o TCC, preferimos priorizar fontes viáveis e documentáveis.',
  },
  {
    name: 'X/Twitter',
    status: 'Futuro',
    description:
      'Deixamos para depois por causa de custo, limites e mudanças frequentes de acesso. A plataforma é relevante, mas não é a melhor base inicial para um protótipo acadêmico.',
  },
]

function ProjectPlan() {
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
          grid
          grid-cols-1
          gap-6
          xl:grid-cols-[1fr_360px]
        ">
          <div>
            <p className="
              mb-2
              text-xs
              font-semibold
              uppercase
              tracking-wide
              text-zinc-500
              dark:text-zinc-400
            ">
              Sobre o TCC
            </p>

            <h2 className="
              text-2xl
              font-bold
              text-zinc-950
              dark:text-white
            ">
              Nosso objetivo com o Syncra
            </h2>

            <p className="
              mt-3
              text-sm
              leading-6
              text-zinc-600
              dark:text-zinc-300
            ">
              Nós estamos desenvolvendo um sistema de social listening voltado
              para pequenas empresas e MEIs. A proposta é reunir sinais públicos
              de diferentes redes, organizar menções, sentimento, engajamento e
              termos recorrentes, e transformar isso em leituras simples para
              apoiar decisões de marca, atendimento e conteúdo.
            </p>
          </div>

          <div className="
            rounded-lg
            bg-zinc-50
            p-5
            dark:bg-zinc-950
          ">
            <div className="
              mb-3
              flex
              items-center
              gap-3
              text-zinc-950
              dark:text-white
            ">
              <Compass size={20} />
              <h3 className="font-bold">
                Recorte do protótipo
              </h3>
            </div>

            <p className="
              text-sm
              leading-6
              text-zinc-600
              dark:text-zinc-300
            ">
              Nesta fase, nosso foco não é substituir ferramentas profissionais,
              mas provar que um painel acessível pode ajudar negócios menores a
              enxergar conversas públicas que antes ficariam espalhadas.
            </p>
          </div>
        </div>
      </section>

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
          <CheckCircle2
            size={22}
            className="text-emerald-600"
          />

          <h2 className="
            text-xl
            font-bold
            text-zinc-950
            dark:text-white
          ">
            O que já fizemos
          </h2>
        </div>

        <div className="
          grid
          grid-cols-1
          gap-3
          md:grid-cols-2
        ">
          {completedItems.map((item) => (
            <p
              key={item}
              className="
                rounded-lg
                bg-zinc-50
                p-4
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
      </section>

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
          <Layers3
            size={22}
            className="text-sky-600"
          />

          <h2 className="
            text-xl
            font-bold
            text-zinc-950
            dark:text-white
          ">
            Por que essas redes
          </h2>
        </div>

        <div className="
          grid
          grid-cols-1
          gap-4
          md:grid-cols-2
          xl:grid-cols-3
        ">
          {sources.map((source) => (
            <SourceCard
              key={source.name}
              source={source}
            />
          ))}
        </div>
      </section>

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
          <Clock3
            size={22}
            className="text-amber-600"
          />

          <h2 className="
            text-xl
            font-bold
            text-zinc-950
            dark:text-white
          ">
            TO DO do projeto
          </h2>
        </div>

        <div className="grid gap-4">
          {todoItems.map((item) => (
            <TodoRow
              key={item.title}
              item={item}
            />
          ))}
        </div>
      </section>
    </div>
  )
}

function SourceCard({ source }) {
  const statusClass =
    source.status === 'Feito'
      ? 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-200'
      : 'bg-amber-50 text-amber-800 dark:bg-amber-950 dark:text-amber-200'

  return (
    <article className="
      rounded-lg
      bg-zinc-50
      p-5
      dark:bg-zinc-950
    ">
      <div className="
        mb-3
        flex
        items-start
        justify-between
        gap-3
      ">
        <h3 className="
          text-lg
          font-bold
          text-zinc-950
          dark:text-white
        ">
          {source.name}
        </h3>

        <span className={`
          rounded-md
          px-2
          py-1
          text-xs
          font-bold
          ${statusClass}
        `}>
          {source.status}
        </span>
      </div>

      <p className="
        text-sm
        leading-6
        text-zinc-600
        dark:text-zinc-300
      ">
        {source.description}
      </p>
    </article>
  )
}

function TodoRow({ item }) {
  return (
    <article className="
      rounded-lg
      bg-zinc-50
      p-4
      dark:bg-zinc-950
    ">
      <div className="
        mb-2
        flex
        flex-col
        gap-2
        sm:flex-row
        sm:items-center
        sm:justify-between
      ">
        <h3 className="
          font-bold
          text-zinc-950
          dark:text-white
        ">
          {item.title}
        </h3>

        <span className="
          w-fit
          rounded-md
          bg-white
          px-2
          py-1
          text-xs
          font-bold
          text-zinc-600
          dark:bg-zinc-900
          dark:text-zinc-300
        ">
          {item.status}
        </span>
      </div>

      <p className="
        text-sm
        leading-6
        text-zinc-600
        dark:text-zinc-300
      ">
        {item.description}
      </p>
    </article>
  )
}

export default ProjectPlan
