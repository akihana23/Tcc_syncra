# Syncra - Social Listening para Pequenos Negocios

Projeto de TCC com uma API em .NET 8 e um frontend React/Vite para monitorar sinais publicos de marcas em redes sociais.

## Estrutura

- `api/`: backend ASP.NET Core com PostgreSQL, JWT, integracoes sociais e insights com OpenAI.
- `web/`: frontend React com dashboard, busca, marcas salvas, snapshots e pagina explicativa do TCC.

## Deploy planejado

- Banco: Neon Postgres
- API: Render
- Frontend: Vercel

As chaves e strings de conexao devem ser configuradas por variaveis de ambiente, nunca commitadas no repositorio.