using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using SocialListening.API.Data;
using SocialListening.API.Models;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SearchController(
            AppDbContext context
        )
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSearch(
            [FromBody] SearchHistory model
        )
        {
            model.Query = model.Query.Trim();

            if (string.IsNullOrWhiteSpace(model.Query))
            {
                return BadRequest(new
                {
                    message = "Informe um termo de busca."
                });
            }

            _context.SearchHistories.Add(model);

            await _context.SaveChangesAsync();

            return Ok(model);
        }
        [HttpDelete]
        public async Task<IActionResult> ClearHistory()
        {
            _context.SearchHistories.RemoveRange(
                _context.SearchHistories
            );

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var history =
                await _context.SearchHistories
                    .OrderByDescending(
                        s => s.CreatedAt
                    )
                    .Take(10)
                    .ToListAsync();

            return Ok(history);


        }
    }
}
