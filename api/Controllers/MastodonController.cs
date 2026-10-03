using Microsoft.AspNetCore.Mvc;
using SocialListening.API.Services;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MastodonController : ControllerBase
    {
        private readonly MastodonService _mastodonService;

        public MastodonController(MastodonService mastodonService)
        {
            _mastodonService = mastodonService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new
                {
                    message = "Informe uma hashtag ou termo de busca."
                });
            }

            try
            {
                return Ok(await _mastodonService.SearchPosts(query));
            }
            catch (Exception ex)
                when (
                    ex is HttpRequestException ||
                    ex is InvalidOperationException ||
                    ex is TaskCanceledException ||
                    ex is System.Text.Json.JsonException
                )
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        message = "Não foi possível buscar dados do Mastodon agora.",
                        detail = ex.Message
                    }
                );
            }
        }
    }
}

