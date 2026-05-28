using Microsoft.AspNetCore.Mvc;
using SocialListening.API.DTOs.AI;
using SocialListening.API.Services;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/ai/insights")]
    public class AiInsightsController : ControllerBase
    {
        private readonly AiInsightService _aiInsightService;

        public AiInsightsController(AiInsightService aiInsightService)
        {
            _aiInsightService = aiInsightService;
        }

        [HttpPost]
        public async Task<IActionResult> Generate(
            [FromBody] AiInsightRequestDto request
        )
        {
            if (
                request is null ||
                string.IsNullOrWhiteSpace(request.Query) ||
                request.Items.Count == 0
            )
            {
                return BadRequest(new
                {
                    message = "Informe a busca e os itens analisados."
                });
            }

            try
            {
                var result =
                    await _aiInsightService
                        .GenerateInsights(request);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is System.Text.Json.JsonException
                )
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        message = "Não foi possível gerar sugestões por IA agora.",
                        detail = ex.Message
                    }
                );
            }
        }
    }
}
