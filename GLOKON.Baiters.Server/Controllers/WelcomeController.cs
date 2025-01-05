using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api")]
    [ApiController]
    public class WelcomeController : ControllerBase
    {
        [HttpGet()]
        public async Task<IActionResult> IndexAsync()
        {
            return Ok("Welcome to Baiters API");
        }
    }
}
