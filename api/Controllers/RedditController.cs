using Microsoft.AspNetCore.Mvc;

using SocialListening.API.Services;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedditController : ControllerBase
    {
        private readonly RedditService _redditService;

        public RedditController(RedditService redditService)
        {
            _redditService = redditService;
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
                var posts = await _redditService.SearchPosts(query);

                return Ok(posts);
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
                        message = "Não foi possível buscar dados do Reddit agora.",
                        detail = ex.Message
                    }
                );
            }
        }
    }
}
