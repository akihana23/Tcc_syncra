import { useEffect, useMemo, useRef, useState } from 'react'

import api from '../services/api'
import {
  buildPlatformData,
  buildSuggestions,
  buildTrends,
  countSentiment,
} from '../utils/socialAnalytics'

import DashboardLayout from '../layouts/DashboardLayout'

import BlueskyPostCard from '../components/BlueskyPostCard'
import BrandWorkspace from '../components/BrandWorkspace'
import ChartCard from '../components/ChartCard'
import EngagementSummaryChart from '../components/EngagementSummaryChart'
import MetricCard from '../components/MetricCard'
import PlatformSentimentChart from '../components/PlatformSentimentChart'
import ProjectPlan from '../components/ProjectPlan'
import ReportButton from '../components/ReportButton'
import RedditPostCard from '../components/RedditPostCard'
import SearchBar from '../components/SearchBar'
import SentimentChart from '../components/SentimentChart'
import SourceDistributionChart from '../components/SourceDistributionChart'
import SuggestionCard from '../components/SuggestionCard'
import Trends from '../components/Trends'
import YoutubeCard from '../components/YoutubeCard'

const viewTitles = {
  overview: 'Visão geral',
  analytics: 'Analytics',
  mentions: 'Menções',
  brands: 'Marcas',
  settings: 'Projeto TCC',
}

function Dashboard({
  darkMode,
  setDarkMode,
}) {
  const [activeView, setActiveView] =
    useState('overview')

  const [search, setSearch] =
    useState('')

  const [searchTerm, setSearchTerm] =
    useState('')

  const [posts, setPosts] =
    useState([])

  const [videos, setVideos] =
    useState([])

  const [blueskyPosts, setBlueskyPosts] =
    useState([])

  const [history, setHistory] =
    useState([])

  const [loading, setLoading] =
    useState(false)

  const [activeTab, setActiveTab] =
    useState('reddit')

  const [sourceErrors, setSourceErrors] =
    useState([])

  const [searchError, setSearchError] =
    useState('')

  const [aiSuggestions, setAiSuggestions] =
    useState([])

  const [aiLoading, setAiLoading] =
    useState(false)

  const aiRequestId = useRef(0)

  useEffect(() => {
    loadHistory()
  }, [])

  async function loadHistory() {
    try {
      const response =
        await api.get('/search')

      setHistory(response.data)

    } catch (error) {
      console.error(error)
    }
  }

  async function clearHistory() {
    try {
      await api.delete('/search')

      setHistory([])

    } catch (error) {
      console.error(error)
    }
  }

  async function handleSearch(e) {
    e.preventDefault()

    const term = search.trim()

    if (!term || loading) return

    setSearch(term)
    setSearchTerm(term)
    setLoading(true)
    setSearchError('')
    setSourceErrors([])
    setAiSuggestions([])
    setAiLoading(false)
    aiRequestId.current += 1

    try {
      const [
        redditResult,
        youtubeResult,
        blueskyResult,
      ] = await Promise.allSettled([
        api.get('/reddit/search', {
          params: {
            query: term,
          },
        }),

        api.get('/youtube/search', {
          params: {
            query: term,
          },
        }),

        api.get('/bluesky/search', {
          params: {
            query: term,
          },
        }),
      ])

      const nextErrors = []
      const nextPosts =
        redditResult.status === 'fulfilled'
          ? redditResult.value.data
          : []
      const nextVideos =
        youtubeResult.status === 'fulfilled'
          ? youtubeResult.value.data
          : []
      const nextBlueskyPosts =
        blueskyResult.status === 'fulfilled'
          ? blueskyResult.value.data
          : []

      if (redditResult.status === 'rejected') {
        nextErrors.push('Reddit')
      }

      if (youtubeResult.status === 'rejected') {
        nextErrors.push('YouTube')
      }

      if (blueskyResult.status === 'rejected') {
        nextErrors.push('BlueSky')
      }

      if (
        redditResult.status === 'rejected' &&
        youtubeResult.status === 'rejected' &&
        blueskyResult.status === 'rejected'
      ) {
        setPosts([])
        setVideos([])
        setBlueskyPosts([])
        setSearchError(
          'Não foi possível carregar dados agora. Tente novamente em alguns instantes.'
        )
        return
      }

      setPosts(nextPosts)
      setVideos(nextVideos)
      setBlueskyPosts(nextBlueskyPosts)
      setSourceErrors(nextErrors)

      void requestAiSuggestions(
        term,
        nextPosts,
        nextVideos,
        nextBlueskyPosts
      )

      await api.post('/search', {
        query: term,
      })

      loadHistory()

    } catch (error) {
      console.error(error)

      setSearchError(
        'Não foi possível concluir a busca. Tente novamente.'
      )

    } finally {
      setLoading(false)
    }
  }

  async function requestAiSuggestions(
    term,
    redditPosts,
    youtubeVideos,
    nextBlueskyPosts
  ) {
    const requestId =
      aiRequestId.current

    const items = buildAiInsightItems(
      redditPosts,
      youtubeVideos,
      nextBlueskyPosts
    )

    if (items.length === 0) {
      return
    }

    setAiLoading(true)

    try {
      const response = await api.post('/ai/insights', {
        query: term,
        items,
      })

      if (aiRequestId.current !== requestId) {
        return
      }

      setAiSuggestions(response.data?.suggestions ?? [])

    } catch (error) {
      console.error(error)

      if (aiRequestId.current === requestId) {
        setAiSuggestions([])
      }

    } finally {
      if (aiRequestId.current === requestId) {
        setAiLoading(false)
      }
    }
  }

  const allContent = useMemo(
    () => [
      ...posts,
      ...videos,
      ...blueskyPosts,
    ],
    [posts, videos, blueskyPosts]
  )

  const totalMentions =
    allContent.length

  const totalScore = posts.reduce(
    (acc, post) => acc + post.score,
    0
  )

  const totalComments = posts.reduce(
    (acc, post) => acc + post.comments,
    0
  )

  const blueskyEngagement = blueskyPosts.reduce(
    (acc, post) => acc + post.likes + post.replies + post.reposts,
    0
  )

  const sentiment = countSentiment(allContent)

  const positivePercentage =
    totalMentions > 0
      ? Math.round(
          (sentiment.positive / totalMentions) * 100
        )
      : 0

  const platformData = useMemo(
    () => buildPlatformData(posts, videos, blueskyPosts),
    [posts, videos, blueskyPosts]
  )

  const trends = useMemo(
    () => buildTrends(allContent),
    [allContent]
  )

  const redditNegative =
    posts.filter(
      post =>
        post.sentiment === 'negative'
    ).length

  const youtubeNegative =
    videos.filter(
      video =>
        video.sentiment === 'negative'
    ).length

  const blueskyNegative =
    blueskyPosts.filter(
      post =>
        post.sentiment === 'negative'
    ).length

  const negativeByPlatform = [
    {
      name: 'Reddit',
      count: redditNegative,
    },
    {
      name: 'YouTube',
      count: youtubeNegative,
    },
    {
      name: 'BlueSky',
      count: blueskyNegative,
    },
  ]

  const activePlatform =
    platformData
      .filter(platform => platform.mentions > 0)
      .sort((a, b) => b.mentions - a.mentions)[0]

  const mostNegative =
    negativeByPlatform
      .filter(platform => platform.count > 0)
      .sort((a, b) => b.count - a.count)[0]

  const mostNegativePlatform =
    totalMentions === 0
      ? 'Sem dados'
      : mostNegative
        ? mostNegative.name
        : 'Sem ruído'

  const mostActivePlatform =
    totalMentions === 0
      ? 'Sem dados'
      : activePlatform?.name ?? 'Equilibrado'

  const localSuggestions = buildSuggestions({
    totalMentions,
    positivePercentage,
    negativeCount: sentiment.negative,
    topTrend: trends[0],
    mostNegativePlatform,
    sourceErrors,
  })

  const suggestions =
    aiSuggestions.length > 0
      ? aiSuggestions
      : localSuggestions

  const suggestionSource =
    aiSuggestions.length > 0
      ? 'IA ativa'
      : 'Regras locais'

  const snapshotPayload = useMemo(
    () => buildBrandSnapshotPayload({
      searchTerm,
      posts,
      videos,
      blueskyPosts,
      platformData,
      trends,
      sentiment,
      suggestions,
      aiSuggestions,
      sourceErrors,
    }),
    [
      searchTerm,
      posts,
      videos,
      blueskyPosts,
      platformData,
      trends,
      sentiment,
      suggestions,
      aiSuggestions,
      sourceErrors,
    ]
  )

  const hasSnapshotData =
    Boolean(searchTerm) &&
    snapshotPayload.items.length > 0

  const activeItems =
    activeTab === 'reddit'
      ? posts
      : activeTab === 'youtube'
        ? videos
        : blueskyPosts

  return (
    <DashboardLayout
      darkMode={darkMode}
      setDarkMode={setDarkMode}
      activeView={activeView}
      onViewChange={setActiveView}
      viewTitle={viewTitles[activeView]}
    >
      <PageIntro searchTerm={searchTerm} />

      {activeView !== 'settings' && (
        <>
          <SearchBar
            search={search}
            setSearch={setSearch}
            handleSearch={handleSearch}
            loading={loading}
          />

          <StatusMessages
            searchError={searchError}
            sourceErrors={sourceErrors}
          />

          {hasSnapshotData && (
            <div className="mb-6 flex justify-end">
              <ReportButton report={snapshotPayload} />
            </div>
          )}
        </>
      )}

      {activeView === 'overview' && (
        <OverviewSection
          totalMentions={totalMentions}
          totalScore={totalScore}
          totalComments={totalComments}
          blueskyEngagement={blueskyEngagement}
          positivePercentage={positivePercentage}
          sentiment={sentiment}
          platformData={platformData}
          trends={trends}
          loading={loading}
          suggestions={suggestions}
          suggestionSource={suggestionSource}
          aiLoading={aiLoading}
          mostNegativePlatform={mostNegativePlatform}
          mostActivePlatform={mostActivePlatform}
        />
      )}

      {activeView === 'analytics' && (
        <AnalyticsSection
          posts={posts}
          blueskyPosts={blueskyPosts}
          platformData={platformData}
          trends={trends}
          loading={loading}
        />
      )}

      {activeView === 'mentions' && (
        <MentionsSection
          activeTab={activeTab}
          setActiveTab={setActiveTab}
          activeItems={activeItems}
          posts={posts}
          videos={videos}
          blueskyPosts={blueskyPosts}
          loading={loading}
          history={history}
          clearHistory={clearHistory}
          setSearch={setSearch}
        />
      )}

      {activeView === 'brands' && (
        <BrandWorkspace
          snapshotPayload={snapshotPayload}
          hasSnapshotData={hasSnapshotData}
        />
      )}

      {activeView === 'settings' && (
        <SettingsSection />
      )}
    </DashboardLayout>
  )
}

function PageIntro({ searchTerm }) {
  return (
    <section className="
      mb-6
      flex
      flex-col
      gap-3
      lg:flex-row
      lg:items-end
      lg:justify-between
    ">
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

        <h1 className="
          mt-1
          text-3xl
          font-bold
          text-zinc-950
          dark:text-white
        ">
          Social listening para pequenos negócios
        </h1>
      </div>

      {searchTerm && (
        <div className="
          rounded-lg
          border
          border-zinc-200
          bg-white
          px-4
          py-3
          text-sm
          text-zinc-600
          shadow-sm
          dark:border-zinc-800
          dark:bg-zinc-900
          dark:text-zinc-300
        ">
          Última busca:
          {' '}
          <strong className="text-zinc-950 dark:text-white">
            {searchTerm}
          </strong>
        </div>
      )}
    </section>
  )
}

function StatusMessages({ searchError, sourceErrors }) {
  return (
    <>
      {searchError && (
        <div className="
          mb-6
          rounded-lg
          border
          border-red-200
          bg-red-50
          px-4
          py-3
          text-sm
          text-red-700
          dark:border-red-900
          dark:bg-red-950/40
          dark:text-red-200
        ">
          {searchError}
        </div>
      )}

      {sourceErrors.length > 0 && !searchError && (
        <div className="
          mb-6
          rounded-lg
          border
          border-amber-200
          bg-amber-50
          px-4
          py-3
          text-sm
          text-amber-800
          dark:border-amber-900
          dark:bg-amber-950/40
          dark:text-amber-200
        ">
          Não foi possível carregar
          {' '}
          {sourceErrors.join(' e ')}
          . Os dados disponíveis continuam na tela.
        </div>
      )}
    </>
  )
}

function OverviewSection({
  totalMentions,
  totalScore,
  totalComments,
  blueskyEngagement,
  positivePercentage,
  sentiment,
  platformData,
  trends,
  loading,
  suggestions,
  suggestionSource,
  aiLoading,
  mostNegativePlatform,
  mostActivePlatform,
}) {
  return (
    <>
      <div className="
        mb-6
        grid
        grid-cols-1
        gap-4
        md:grid-cols-2
        xl:grid-cols-5
      ">
        <MetricCard
          title="Menções totais"
          value={totalMentions}
          growth="3 fontes"
        />

        <MetricCard
          title="Upvotes Reddit"
          value={totalScore}
          growth="Engajamento"
        />

        <MetricCard
          title="Comentários"
          value={totalComments}
          growth="Discussão"
        />

        <MetricCard
          title="Interações BlueSky"
          value={blueskyEngagement}
          growth="Likes + replies"
        />

        <MetricCard
          title="Positividade"
          value={`${positivePercentage}%`}
          growth={`${sentiment.positive} positivas`}
        />
      </div>

      <div className="
        mb-6
        grid
        grid-cols-1
        gap-6
        xl:grid-cols-[1.15fr_0.85fr]
      ">
        <SentimentChart
          positive={sentiment.positive}
          negative={sentiment.negative}
          neutral={sentiment.neutral}
        />

        <QuickRead
          totalMentions={totalMentions}
          positivePercentage={positivePercentage}
          mostNegativePlatform={mostNegativePlatform}
          mostActivePlatform={mostActivePlatform}
          trends={trends}
        />
      </div>

      <div className="
        grid
        grid-cols-1
        gap-6
        xl:grid-cols-3
      ">
        <SourceDistributionChart data={platformData} />

        <Trends
          trends={trends}
          loading={loading}
        />

        <SuggestionCard
          suggestions={suggestions}
          sourceLabel={suggestionSource}
          loading={aiLoading}
        />
      </div>
    </>
  )
}

function QuickRead({
  totalMentions,
  positivePercentage,
  mostNegativePlatform,
  mostActivePlatform,
  trends,
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
      <h2 className="
        mb-5
        text-xl
        font-bold
        text-zinc-950
        dark:text-white
      ">
        Leitura rápida
      </h2>

      <div className="space-y-4">
        <InsightRow
          label="Percepção geral"
          value={
            totalMentions > 0
              ? `${positivePercentage}% positivas`
              : 'Aguardando busca'
          }
        />

        <InsightRow
          label="Maior volume negativo"
          value={mostNegativePlatform}
        />

        <InsightRow
          label="Plataforma mais ativa"
          value={mostActivePlatform}
        />

        <InsightRow
          label="Principal termo"
          value={
            trends[0]
              ? `#${trends[0].keyword}`
              : 'Aguardando dados'
          }
        />
      </div>
    </section>
  )
}

function AnalyticsSection({
  posts,
  platformData,
  trends,
  loading,
}) {
  return (
    <div className="space-y-6">
      <div className="
        grid
        grid-cols-1
        gap-6
        xl:grid-cols-2
      ">
        <PlatformSentimentChart data={platformData} />
        <EngagementSummaryChart data={platformData} />
      </div>

      <div className="
        grid
        grid-cols-1
        gap-6
        xl:grid-cols-2
      ">
        <ChartCard posts={posts} />

        <Trends
          trends={trends}
          loading={loading}
        />
      </div>
    </div>
  )
}

function MentionsSection({
  activeTab,
  setActiveTab,
  activeItems,
  posts,
  videos,
  blueskyPosts,
  loading,
  history,
  clearHistory,
  setSearch,
}) {
  return (
    <div className="
      grid
      grid-cols-1
      gap-6
      xl:grid-cols-[1fr_320px]
    ">
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
          mb-6
          flex
          flex-col
          gap-4
          md:flex-row
          md:items-center
          md:justify-between
        ">
          <h2 className="
            text-xl
            font-bold
            text-zinc-950
            dark:text-white
          ">
            Menções encontradas
          </h2>

          <div className="
            flex
            rounded-lg
            bg-zinc-100
            p-1
            dark:bg-zinc-950
          ">
            <TabButton
              active={activeTab === 'reddit'}
              onClick={() => setActiveTab('reddit')}
            >
              Reddit ({posts.length})
            </TabButton>

            <TabButton
              active={activeTab === 'youtube'}
              onClick={() => setActiveTab('youtube')}
              tone="red"
            >
              YouTube ({videos.length})
            </TabButton>

            <TabButton
              active={activeTab === 'bluesky'}
              onClick={() => setActiveTab('bluesky')}
              tone="sky"
            >
              BlueSky ({blueskyPosts.length})
            </TabButton>
          </div>
        </div>

        {loading && (
          <EmptyMentions>
            Carregando dados...
          </EmptyMentions>
        )}

        {!loading && activeItems.length === 0 && (
          <EmptyMentions>
            Faça uma busca para visualizar menções.
          </EmptyMentions>
        )}

        {!loading &&
          activeTab === 'reddit' &&
          posts.length > 0 && (
            <div className="grid gap-4">
              {posts.map(
                (post, index) => (
                  <RedditPostCard
                    key={`${post.url}-${index}`}
                    post={post}
                  />
                )
              )}
            </div>
          )}

        {!loading &&
          activeTab === 'youtube' &&
          videos.length > 0 && (
            <div className="
              grid
              grid-cols-1
              gap-4
              md:grid-cols-2
            ">
              {videos.map(
                (video, index) => (
                  <YoutubeCard
                    key={`${video.videoId}-${index}`}
                    video={video}
                  />
                )
              )}
            </div>
          )}

        {!loading &&
          activeTab === 'bluesky' &&
          blueskyPosts.length > 0 && (
            <div className="grid gap-4">
              {blueskyPosts.map(
                (post, index) => (
                  <BlueskyPostCard
                    key={`${post.url}-${index}`}
                    post={post}
                  />
                )
              )}
            </div>
          )}
      </section>

      <HistoryPanel
        history={history}
        clearHistory={clearHistory}
        setSearch={setSearch}
      />
    </div>
  )
}

function TabButton({
  active,
  onClick,
  children,
  tone = 'orange',
}) {
  const activeColor =
    tone === 'red'
      ? 'text-red-600'
      : tone === 'sky'
        ? 'text-sky-600'
        : 'text-orange-600'

  return (
    <button
      onClick={onClick}
      className={`
        rounded-md
        px-4
        py-2
        text-sm
        font-semibold
        transition

        ${
          active
            ? `bg-white ${activeColor} shadow-sm dark:bg-zinc-800`
            : 'text-zinc-600 dark:text-zinc-300'
        }
      `}
      type="button"
    >
      {children}
    </button>
  )
}

function EmptyMentions({ children }) {
  return (
    <div className="
      rounded-lg
      border
      border-dashed
      border-zinc-300
      p-8
      text-center
      text-zinc-500
      dark:border-zinc-700
      dark:text-zinc-400
    ">
      {children}
    </div>
  )
}

function HistoryPanel({
  history,
  clearHistory,
  setSearch,
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
        justify-between
        gap-3
      ">
        <h2 className="
          text-xl
          font-bold
          text-zinc-950
          dark:text-white
        ">
          Histórico
        </h2>

        <button
          onClick={clearHistory}
          disabled={history.length === 0}
          className="
            rounded-lg
            bg-red-500
            px-3
            py-2
            text-sm
            font-semibold
            text-white
            transition
            hover:bg-red-600
            disabled:cursor-not-allowed
            disabled:opacity-40
          "
          type="button"
        >
          Limpar
        </button>
      </div>

      <div className="
        flex
        max-h-96
        flex-wrap
        gap-2
        overflow-y-auto
      ">
        {history.length === 0 && (
          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            Nenhuma busca salva ainda.
          </p>
        )}

        {history.map(item => (
          <button
            key={item.id}
            onClick={() =>
              setSearch(item.query)
            }
            className="
              rounded-lg
              border
              border-zinc-200
              bg-zinc-50
              px-3
              py-2
              text-sm
              text-zinc-700
              transition
              hover:border-zinc-400
              dark:border-zinc-800
              dark:bg-zinc-950
              dark:text-zinc-200
            "
            type="button"
          >
            {item.query}
          </button>
        ))}
      </div>
    </section>
  )
}

function SettingsSection() {
  return (
    <ProjectPlan />
  )
}

function InsightRow({ label, value }) {
  return (
    <div className="
      flex
      items-center
      justify-between
      gap-4
      border-b
      border-zinc-100
      pb-3
      last:border-b-0
      last:pb-0
      dark:border-zinc-800
    ">
      <span className="text-sm text-zinc-500 dark:text-zinc-400">
        {label}
      </span>

      <strong className="
        text-right
        text-sm
        text-zinc-950
        dark:text-white
      ">
        {value}
      </strong>
    </div>
  )
}

function buildAiInsightItems(posts, videos, blueskyPosts) {
  return buildListeningItems(posts, videos, blueskyPosts)
    .map(({ source, title, sentiment, engagement }) => ({
      source,
      title,
      sentiment,
      engagement,
    }))
}

function buildBrandSnapshotPayload({
  searchTerm,
  posts,
  videos,
  blueskyPosts,
  platformData,
  trends,
  sentiment,
  suggestions,
  aiSuggestions,
  sourceErrors,
}) {
  const items =
    buildListeningItems(posts, videos, blueskyPosts)

  return {
    query: searchTerm,
    totalMentions: items.length,
    engagement: items.reduce(
      (acc, item) => acc + item.engagement,
      0
    ),
    positive: sentiment.positive,
    negative: sentiment.negative,
    neutral: sentiment.neutral,
    platforms: platformData.map((platform) => ({
      source: platform.name,
      mentions: platform.mentions,
      engagement: platform.engagement,
      positive: platform.positive,
      negative: platform.negative,
      neutral: platform.neutral,
    })),
    topTerms: trends.slice(0, 10).map((trend) => ({
      term: trend.keyword,
      count: trend.count,
    })),
    items,
    recommendations: suggestions,
    aiInsights: aiSuggestions,
    sourceErrors,
  }
}

function buildListeningItems(posts, videos, blueskyPosts) {
  return [
    ...posts.map(post => ({
      source: 'Reddit',
      title: post.title,
      url: post.url,
      sentiment: post.sentiment,
      engagement:
        Number(post.score ?? 0) +
        Number(post.comments ?? 0),
    })),
    ...videos.map(video => ({
      source: 'YouTube',
      title: video.title,
      url:
        video.url ??
        (video.videoId
          ? `https://youtube.com/watch?v=${video.videoId}`
          : ''),
      sentiment: video.sentiment,
      engagement: 1,
    })),
    ...blueskyPosts.map(post => ({
      source: 'BlueSky',
      title: post.title,
      url: post.url,
      sentiment: post.sentiment,
      engagement:
        Number(post.likes ?? 0) +
        Number(post.replies ?? 0) +
        Number(post.reposts ?? 0),
    })),
  ].filter(item => item.title)
}

export default Dashboard
