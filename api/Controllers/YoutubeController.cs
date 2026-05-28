using Microsoft.AspNetCore.Mvc;

using SocialListening.API.Services;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YoutubeController : ControllerBase
    {
        private readonly YoutubeService _youtubeService;

        public YoutubeController(
            YoutubeService youtubeService
        )
        {
            _youtubeService = youtubeService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query
        )
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new
                {
                    message = "Informe um termo de busca."
                });
            }

            try
            {
                var videos =
                    await _youtubeService
                        .SearchVideos(query);

                return Ok(videos);
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is TaskCanceledException ||
                    ex is InvalidOperationException ||
                    ex is System.Text.Json.JsonException
                )
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        message = "Não foi possível buscar dados do YouTube agora.",
                        detail = ex.Message
                    }
                );
            }
        }
    }
}
