using GLOKON.Baiters.Core;
using GLOKON.Baiters.Server.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/servers")]
    [ApiController]
    [Authorize(Policy = "SteamAdmin")]
    public class ServerController(GameManager gm) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new ServerInfoResponse
            {
                ServerSteamId = gm.Server.ServerId.ToString(),
                LobbyCode = gm.Server.LobbyCode,
                PlayerCount = gm.Server.PlayerCount,
                MaxPlayers = gm.Server.MaxPlayerCount,
            });
        }
    }
}
