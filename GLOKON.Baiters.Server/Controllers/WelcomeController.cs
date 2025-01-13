using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api")]
    [ApiController]
    public class WelcomeController : ControllerBase
    {
        [HttpGet()]
        public IActionResult IndexAsync()
        {
            return Ok("Welcome to Baiters API");
        }
    }
}
