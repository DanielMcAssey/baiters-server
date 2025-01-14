using GLOKON.Baiters.Core;
using GLOKON.Baiters.Core.Models.Game;
using GLOKON.Baiters.Server.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Policy = "SteamAdmin")]
    public class UsersController(GameManager gm) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(gm.Server.Players);
        }

        [HttpPost("kick/{steamId}")]
        public IActionResult KickPlayer([FromRoute]string steamId)
        {
            if (ulong.TryParse(steamId, out ulong parsedSteamId) && gm.Server.TryGetPlayer(parsedSteamId, out var player) && player != null)
            {
                gm.Server.KickPlayer(parsedSteamId);
                return Ok();
            }

            return NotFound();
        }

        [HttpPost("ban/{steamId}")]
        public IActionResult BanPlayer([FromRoute] string steamId)
        {
            if (ulong.TryParse(steamId, out ulong parsedSteamId) && gm.Server.TryGetPlayer(parsedSteamId, out var player) && player != null)
            {
                gm.Server.BanPlayer(parsedSteamId);
                return Ok();
            }

            return NotFound();
        }

        [HttpPost("letter/{steamId?}")]
        public IActionResult SendPlayerLetter([FromRoute] string? steamId, [FromBody] SendLetterRequest request)
        {
            var items = request.Items?.ToArray();
            if (!string.IsNullOrWhiteSpace(steamId))
            {
                gm.Actioner.SendLetter(ulong.Parse(steamId), request.Header, request.Body, request.Closing, items);
            }
            else
            {
                foreach (var player in gm.Server.Players)
                {
                    gm.Actioner.SendLetter(player.Key, request.Header, request.Body, request.Closing, items);
                }
            }

            return Ok();
        }

        [HttpPost("cosmetics/{steamId?}")]
        public IActionResult SetPlayerCosmetics([FromRoute] string? steamId, [FromBody] Cosmetics request)
        {
            if (!string.IsNullOrWhiteSpace(steamId))
            {
                gm.Actioner.SetPlayerCosmetics(ulong.Parse(steamId), request);
            }
            else
            {
                foreach (var player in gm.Server.Players)
                {
                    gm.Actioner.SetPlayerCosmetics(player.Key, request);
                }
            }

            return Ok();
        }

        [HttpPost("held-item/{steamId?}")]
        public IActionResult SetPlayerHeldItem([FromRoute] string? steamId, [FromBody] HeldItem request)
        {
            if (!string.IsNullOrWhiteSpace(steamId))
            {
                gm.Actioner.SetPlayerHeldItem(ulong.Parse(steamId), request);
            }
            else
            {
                foreach (var player in gm.Server.Players)
                {
                    gm.Actioner.SetPlayerHeldItem(player.Key, request);
                }
            }

            return Ok();
        }
    }
}
