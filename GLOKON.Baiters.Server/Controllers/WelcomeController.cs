using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize(Policy = "SteamAdmin")]
    public class WelcomeController : ControllerBase
    {
        [HttpGet()]
        public IActionResult IndexAsync()
        {
            return Ok("Welcome to Baiters API");
        }
    }
}
