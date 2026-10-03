# Deploy do Syncra

Este projeto foi organizado para publicar:

- Banco de dados no Neon Postgres
- API ASP.NET Core no Render
- Frontend React/Vite na Vercel

## 1. Neon Postgres

Crie um projeto no Neon e copie a string de conexao pooled ou direct do banco principal.

No Render, use essa string na variavel:

```text
ConnectionStrings__DefaultConnection
```

## 2. Render

Use o Blueprint do arquivo `render.yaml`.

Variaveis para preencher no painel do Render:

```text
ConnectionStrings__DefaultConnection=<string do Neon>
Jwt__Key=<segredo longo para JWT>
OpenAI__ApiKey=<sua chave da OpenAI>
Youtube__ApiKey=<sua chave do YouTube>
Mastodon__AccessToken=<opcional; use apenas se uma instancia exigir autenticacao>
CORS_ALLOWED_ORIGINS=<url do frontend na Vercel>
```

A integracao do Mastodon consulta as timelines publicas de hashtags de `mastodon.social` e `mastodon.world`. A chave `Mastodon__AccessToken` e opcional e so e necessaria se alguma instancia exigir autenticacao para mostrar dados publicos.

Enquanto a URL final da Vercel ainda nao existir, use temporariamente:

```text
CORS_ALLOWED_ORIGINS=http://localhost:5173
```

Depois do deploy do frontend, volte ao Render e troque pelo dominio da Vercel.

## 3. Vercel

Crie um projeto apontando para a pasta `web`.

Configuracao esperada:

```text
Framework Preset: Vite
Root Directory: web
Build Command: npm run build
Output Directory: dist
Install Command: npm ci
```

Variavel de ambiente:

```text
VITE_API_URL=https://sua-api-no-render.onrender.com/api
```

## 4. Conferencia

Teste estes pontos:

- `https://sua-api-no-render.onrender.com/api/test`
- Tela de login/cadastro do frontend
- Busca social no dashboard
- Guia Marcas, salvando snapshot e comparando marcas
- Guia Projeto TCC
