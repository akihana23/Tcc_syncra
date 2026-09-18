import { FileDown } from 'lucide-react'

function ReportButton({ report }) {
  const hasData =
    Boolean(report?.query) &&
    report.items.length > 0

  function handleGenerateReport() {
    if (!hasData) return

    const reportWindow = window.open('', '_blank')

    if (!reportWindow) {
      window.alert(
        'O navegador bloqueou a nova janela do relatório. Permita pop-ups para este site e tente novamente.'
      )
      return
    }

    reportWindow.document.write(
      buildReportDocument(report)
    )
    reportWindow.document.close()

    reportWindow.onload = () => {
      reportWindow.focus()
      reportWindow.print()
    }
  }

  return (
    <button
      className="
        inline-flex
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
        hover:bg-zinc-700
        focus-visible:outline-2
        focus-visible:outline-offset-2
        focus-visible:outline-zinc-950
        disabled:cursor-not-allowed
        disabled:bg-zinc-300
        disabled:text-zinc-500
        dark:bg-white
        dark:text-zinc-950
        dark:hover:bg-zinc-200
        dark:disabled:bg-zinc-800
        dark:disabled:text-zinc-500
      "
      disabled={!hasData}
      onClick={handleGenerateReport}
      title={
        hasData
          ? 'Gerar relatório da pesquisa atual'
          : 'Faça uma busca com resultados para gerar o relatório'
      }
      type="button"
    >
      <FileDown size={18} aria-hidden="true" />
      Gerar relatório
    </button>
  )
}

function buildReportDocument(report) {
  const generatedAt = new Intl.DateTimeFormat(
    'pt-BR',
    {
      dateStyle: 'long',
      timeStyle: 'short',
    }
  ).format(new Date())

  const positivePercentage =
    report.totalMentions > 0
      ? Math.round(
          (report.positive / report.totalMentions) * 100
        )
      : 0

  const availablePlatforms =
    report.platforms.filter(
      platform => platform.mentions > 0
    )

  const failedSources =
    report.sourceErrors.length > 0
      ? report.sourceErrors.join(', ')
      : 'Nenhuma fonte apresentou erro na coleta.'

  return `<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <title>Relatório Syncra - ${escapeHtml(report.query)}</title>
  <style>
    @page { margin: 18mm; }
    * { box-sizing: border-box; }
    body {
      color: #18181b;
      font-family: Arial, Helvetica, sans-serif;
      font-size: 12px;
      line-height: 1.5;
      margin: 0;
    }
    h1, h2, h3, p { margin-top: 0; }
    h1 { font-size: 28px; letter-spacing: 0; margin-bottom: 4px; }
    h2 {
      border-bottom: 1px solid #d4d4d8;
      font-size: 16px;
      margin: 26px 0 12px;
      padding-bottom: 7px;
    }
    .eyebrow {
      color: #e11d48;
      font-size: 11px;
      font-weight: 700;
      letter-spacing: .08em;
      margin-bottom: 8px;
      text-transform: uppercase;
    }
    .muted { color: #52525b; }
    .header {
      border-bottom: 3px solid #e11d48;
      padding-bottom: 18px;
    }
    .stats {
      display: grid;
      gap: 10px;
      grid-template-columns: repeat(4, 1fr);
      margin-top: 18px;
    }
    .stat {
      background: #f4f4f5;
      border: 1px solid #e4e4e7;
      padding: 12px;
    }
    .stat-label {
      color: #52525b;
      display: block;
      font-size: 10px;
      margin-bottom: 4px;
      text-transform: uppercase;
    }
    .stat-value { font-size: 20px; font-weight: 700; }
    table { border-collapse: collapse; width: 100%; }
    th {
      background: #27272a;
      color: #fff;
      font-size: 10px;
      letter-spacing: .04em;
      padding: 8px;
      text-align: left;
      text-transform: uppercase;
    }
    td {
      border-bottom: 1px solid #e4e4e7;
      padding: 8px;
      vertical-align: top;
    }
    .pill {
      background: #f4f4f5;
      border: 1px solid #d4d4d8;
      display: inline-block;
      margin: 0 6px 6px 0;
      padding: 4px 8px;
    }
    .recommendations {
      margin: 0;
      padding-left: 18px;
    }
    .recommendations li { margin-bottom: 7px; }
    .note {
      background: #fff7ed;
      border-left: 3px solid #f97316;
      margin-top: 20px;
      padding: 12px;
    }
    a { color: #be123c; text-decoration: none; }
    .footer {
      border-top: 1px solid #d4d4d8;
      color: #71717a;
      font-size: 10px;
      margin-top: 28px;
      padding-top: 12px;
    }
    @media print {
      a { color: #18181b; }
      .avoid-break { break-inside: avoid; }
    }
  </style>
</head>
<body>
  <header class="header">
    <p class="eyebrow">Syncra · Social listening acessível</p>
    <h1>Relatório de pesquisa</h1>
    <p class="muted">Termo analisado: <strong>${escapeHtml(report.query)}</strong><br>Gerado em ${escapeHtml(generatedAt)}</p>
  </header>

  <section class="stats avoid-break">
    <div class="stat"><span class="stat-label">Menções</span><span class="stat-value">${report.totalMentions}</span></div>
    <div class="stat"><span class="stat-label">Engajamento</span><span class="stat-value">${report.engagement}</span></div>
    <div class="stat"><span class="stat-label">Positividade</span><span class="stat-value">${positivePercentage}%</span></div>
    <div class="stat"><span class="stat-label">Menções negativas</span><span class="stat-value">${report.negative}</span></div>
  </section>

  <section class="avoid-break">
    <h2>Cobertura da coleta</h2>
    <table>
      <thead><tr><th>Fonte</th><th>Menções</th><th>Engajamento</th><th>Positivas</th><th>Negativas</th></tr></thead>
      <tbody>
        ${availablePlatforms.length > 0
          ? availablePlatforms.map(platform => `<tr><td>${escapeHtml(platform.source)}</td><td>${platform.mentions}</td><td>${platform.engagement}</td><td>${platform.positive}</td><td>${platform.negative}</td></tr>`).join('')
          : '<tr><td colspan="5">Nenhuma fonte retornou menções para esta pesquisa.</td></tr>'}
      </tbody>
    </table>
  </section>

  <section class="avoid-break">
    <h2>Termos recorrentes</h2>
    <div>
      ${report.topTerms.length > 0
        ? report.topTerms.map(term => `<span class="pill">#${escapeHtml(term.term)} · ${term.count}</span>`).join('')
        : '<p class="muted">Não houve termos suficientes para identificar recorrências.</p>'}
    </div>
  </section>

  <section class="avoid-break">
    <h2>Insights e recomendações</h2>
    ${report.recommendations.length > 0
      ? `<ul class="recommendations">${report.recommendations.map(item => `<li>${escapeHtml(item)}</li>`).join('')}</ul>`
      : '<p class="muted">Não foram geradas recomendações para esta pesquisa.</p>'}
  </section>

  <section>
    <h2>Menções analisadas</h2>
    <table>
      <thead><tr><th>Fonte</th><th>Menção</th><th>Sentimento</th><th>Engajamento</th></tr></thead>
      <tbody>
        ${report.items.slice(0, 30).map(item => `<tr><td>${escapeHtml(item.source)}</td><td>${item.url ? `<a href="${escapeAttribute(item.url)}">${escapeHtml(item.title)}</a>` : escapeHtml(item.title)}</td><td>${escapeHtml(translateSentiment(item.sentiment))}</td><td>${item.engagement}</td></tr>`).join('')}
      </tbody>
    </table>
  </section>

  <aside class="note">
    <strong>Limitação da coleta:</strong> este relatório considera somente as fontes que responderam no momento da pesquisa. Fontes indisponíveis: ${escapeHtml(failedSources)}. Os resultados são indicativos e devem ser interpretados junto ao contexto da marca, do público e do período analisado.
  </aside>

  <footer class="footer">
    Relatório gerado pelo Syncra para fins de monitoramento e análise de presença digital.
  </footer>
</body>
</html>`
}

function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;')
}

function escapeAttribute(value) {
  return escapeHtml(value)
    .replaceAll(' ', '%20')
}

function translateSentiment(sentiment) {
  const labels = {
    positive: 'Positivo',
    negative: 'Negativo',
    neutral: 'Neutro',
  }

  return labels[sentiment] ?? 'Neutro'
}

export default ReportButton
