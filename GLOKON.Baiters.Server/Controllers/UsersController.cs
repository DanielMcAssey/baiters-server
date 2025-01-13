using GLOKON.Baiters.Core;
using GLOKON.Baiters.Core.Models.Game;
using GLOKON.Baiters.Server.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/users")]
    [ApiController]
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

        [HttpPost("message/{steamId?}")]
        public IActionResult SendPlayerMessage([FromRoute] string? steamId, [FromBody] SendMessageRequest request)
        {
            ulong? steamIdToMessage = null;
            if (!string.IsNullOrWhiteSpace(steamId))
            {
                steamIdToMessage = ulong.Parse(steamId);
            }

            gm.Server.SendMessage(request.Message, request.Colour, steamIdToMessage);

            return Ok();
        }

        [HttpPost("letter/{steamId}")]
        public IActionResult SendPlayerLetter([FromRoute] string steamId, [FromBody] SendLetterRequest request)
        {
            gm.Actioner.SendLetter(ulong.Parse(steamId), request.Header, request.Body, request.Closing, request.Items?.ToArray());

            return Ok();
        }

        [HttpPost("cosmetics/{steamId}")]
        public IActionResult SetPlayerCosmetics([FromRoute] string steamId, [FromBody] Cosmetics request)
        {
            gm.Actioner.SetPlayerCosmetics(ulong.Parse(steamId), request);

            return Ok();
        }

        [HttpPost("held-item/{steamId}")]
        public IActionResult SetPlayerHeldItem([FromRoute] string steamId, [FromBody] HeldItem request)
        {
            gm.Actioner.SetPlayerHeldItem(ulong.Parse(steamId), request);

            return Ok();
        }
    }
}
