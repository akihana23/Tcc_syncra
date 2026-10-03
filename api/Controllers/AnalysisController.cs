using Microsoft.AspNetCore.Mvc;
using SocialListening.API.DTOs.Analysis;
using SocialListening.API.Services;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/analysis")]
    public class AnalysisController : ControllerBase
    {
        private readonly SocialListeningAnalysisService _analysisService;

        public AnalysisController(
            SocialListeningAnalysisService analysisService
        )
        {
            _analysisService = analysisService;
        }

        [HttpGet("topic")]
        public async Task<IActionResult> AnalyzeTopic(
            [FromQuery] string query,
            [FromQuery] string[]? brands
        )
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new
                {
                    message = "Informe um termo de busca."
                });
            }

            var result =
                await _analysisService.AnalyzeTopic(query, brands);

            return Ok(result);
        }

        [HttpGet("roadmap")]
        public IActionResult GetRoadmap()
        {
            return Ok(new RoadmapResponseDto
            {
                GeneratedAt = DateTimeOffset.UtcNow,
                Items =
                [
                    new RoadmapItemDto
                    {
                        Title = "Fontes sociais principais",
                        Status = "Feito",
                        Priority = "Alta",
                        Description = "Mastodon, YouTube e BlueSky integrados como fontes iniciais de social listening."
                    },
                    new RoadmapItemDto
                    {
                        Title = "Relevancia de marca",
                        Status = "Feito",
                        Priority = "Alta",
                        Description = "Filtro de contexto para reduzir falsos positivos em marcas curtas ou ambiguas, como TIM."
                    },
                    new RoadmapItemDto
                    {
                        Title = "Comparacao entre marcas",
                        Status = "Feito na API",
                        Priority = "Alta",
                        Description = "Endpoint compara volume, sentimento, engajamento, plataformas e termos entre concorrentes."
                    },
                    new RoadmapItemDto
                    {
                        Title = "Busca por termo de mercado",
                        Status = "Feito na API",
                        Priority = "Alta",
                        Description = "Endpoint analisa termos como bolo, identifica assuntos recorrentes e possiveis marcas citadas."
                    },
                    new RoadmapItemDto
                    {
                        Title = "Relatorios e exportacao",
                        Status = "Parcial",
                        Priority = "Media",
                        Description = "Exportacao CSV disponivel; PDF pode entrar como melhoria para apresentacao e uso por pequenos negocios."
                    },
                    new RoadmapItemDto
                    {
                        Title = "Cadastro de marca monitorada",
                        Status = "Proximo",
                        Priority = "Alta",
                        Description = "Criar tela para salvar nome, aliases, cidade, categoria, concorrentes e termos de contexto."
                    },
                    new RoadmapItemDto
                    {
                        Title = "Limites e validacao academica",
                        Status = "Proximo",
                        Priority = "Media",
                        Description = "Documentar limitacoes das APIs, privacidade, falsos positivos, custos e criterios de avaliacao do TCC."
                    }
                ]
            });
        }

        [HttpGet("compare")]
        public async Task<IActionResult> CompareBrands(
            [FromQuery] string[] brands
        )
        {
            if (brands.Length < 2)
            {
                return BadRequest(new
                {
                    message = "Informe pelo menos duas marcas para comparar."
                });
            }

            var result =
                await _analysisService.CompareBrands(brands);

            return Ok(result);
        }

        [HttpGet("topic/export")]
        public async Task<IActionResult> ExportTopicCsv(
            [FromQuery] string query,
            [FromQuery] string[]? brands
        )
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new
                {
                    message = "Informe um termo de busca."
                });
            }

            var result =
                await _analysisService.AnalyzeTopic(query, brands);

            var csv =
                SocialListeningAnalysisService.BuildTopicCsv(result);

            return File(
                System.Text.Encoding.UTF8.GetBytes(csv),
                "text/csv",
                "syncra-topic-analysis.csv"
            );
        }

        [HttpGet("compare/export")]
        public async Task<IActionResult> ExportComparisonCsv(
            [FromQuery] string[] brands
        )
        {
            if (brands.Length < 2)
            {
                return BadRequest(new
                {
                    message = "Informe pelo menos duas marcas para comparar."
                });
            }

            var result =
                await _analysisService.CompareBrands(brands);

            var csv =
                SocialListeningAnalysisService.BuildBrandComparisonCsv(result);

            return File(
                System.Text.Encoding.UTF8.GetBytes(csv),
                "text/csv",
                "syncra-brand-comparison.csv"
            );
        }
    }
}

