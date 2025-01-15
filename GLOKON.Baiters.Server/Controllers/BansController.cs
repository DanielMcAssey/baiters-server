using GLOKON.Baiters.Core;
using GLOKON.Baiters.Server.Requests;
using GLOKON.Baiters.Server.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/bans")]
    [ApiController]
    [Authorize(Policy = "SteamAdmin")]
    public class BansController(GameManager gm) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(gm.Server.PlayerBans.Select((ban) => new BanResponse(ban.Key, ban.Value)).ToList().OrderByDescending(ban => ban.CreatedAt));
        }

        [HttpPost("{steamId}")]
        public IActionResult Add([FromRoute] ulong steamId, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] NewBanRequest? newBan)
        {
            gm.Server.BanPlayer(steamId, newBan?.Reason);
            return Ok();
        }

        [HttpDelete("{steamId}")]
        public IActionResult Delete([FromRoute] ulong steamId)
        {
            gm.Server.UnbanPlayer(steamId);
            return NoContent();
        }
    }
}
