using GLOKON.Baiters.Core;
using GLOKON.Baiters.Core.Constants;
using GLOKON.Baiters.Core.Models.Chat;
using GLOKON.Baiters.Server.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/chats")]
    [ApiController]
    [Authorize(Policy = "SteamAdmin")]
    public class ChatsController(GameManager gm) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // TODO: Add support for chat logs
            return Ok(new List<ChatLog>
            {
                new()
                {
                    SenderId = gm.Server.ServerId,
                    SenderName = "Server",
                    Message = "Chat Window coming soon…",
                    Colour = MessageColour.Error,
                }
            });
        }

        [HttpPost("messages/{steamId?}")]
        public IActionResult SendMessage([FromRoute] string? steamId, [FromBody] SendMessageRequest request)
        {
            ulong? steamIdToMessage = null;
            if (!string.IsNullOrWhiteSpace(steamId))
            {
                steamIdToMessage = ulong.Parse(steamId);
            }

            gm.Server.SendMessage(request.Message, request.Colour, steamIdToMessage);

            return Ok();
        }
    }
}
