using Microsoft.AspNetCore.Mvc;
using SocialListening.API.Services;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendsController : ControllerBase
    {
        private readonly TrendService _trendService;

        public TrendsController(TrendService trendService)
        {
            _trendService = trendService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrends(
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

            var result = await _trendService.GetTrends(query);

            return Ok(result);
        }
    }
}
