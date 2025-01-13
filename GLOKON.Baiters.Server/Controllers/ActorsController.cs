using GLOKON.Baiters.Core;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/actors")]
    [ApiController]
    public class ActorsController(GameManager gm) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(gm.Server.Actors);
        }
    }
}
