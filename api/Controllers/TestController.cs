using Microsoft.AspNetCore.Mvc;

namespace SocialListening.API.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "API funcionando perfeitamente"
            });
        }
    }
}