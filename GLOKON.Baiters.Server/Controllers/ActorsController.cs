using GLOKON.Baiters.Core;
using GLOKON.Baiters.Server.Responses;
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
            return Ok(gm.Server.Actors.Select((actor) => new ActorResponse(actor.Key, actor.Value)).ToList().OrderByDescending(actor => actor.SpawnedAt));
        }

        [HttpGet("spawn/types")]
        public IActionResult GetTypes()
        {
            return Ok(ActorSpawner.Spawnable);
        }

        [HttpPost("spawn/{type}")]
        public IActionResult SpawnType([FromRoute] string type)
        {
            if (gm.Spawner.Spawn(type))
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] long id)
        {
            if (gm.Server.RemoveActor(id))
            {
                return NoContent();
            }

            return BadRequest();
        }
    }
}
