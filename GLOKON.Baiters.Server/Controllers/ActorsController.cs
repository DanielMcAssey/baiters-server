using GLOKON.Baiters.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/actors")]
    [ApiController]
    [Authorize(Policy = "SteamAdmin")]
    public class ActorsController(GameManager gm) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(gm.Server.Actors);
        }

        [HttpGet("spawn/types")]
        public IActionResult GetTypes()
        {
            return Ok(ActorSpawner.Spawnable);
        }

        [HttpPost("spawn/{type}")]
        public IActionResult Delete([FromRoute] string type)
        {
            gm.Spawner.Spawn(type);
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] long id)
        {
            gm.Server.RemoveActor(id);
            return NoContent();
        }
    }
}
