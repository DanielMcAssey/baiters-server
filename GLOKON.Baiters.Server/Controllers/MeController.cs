using AspNet.Security.OpenId.Steam;
using GLOKON.Baiters.Core.Configuration;
using GLOKON.Baiters.Server.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/me")]
    [ApiController]
    [Authorize(Policy = "AnySteam")]
    public class MeController(IOptions<WebFishingOptions> options) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var steamId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(steamId))
            {
                string cleanSteamId = steamId.Replace(SteamAuthenticationConstants.Namespaces.Identifier, string.Empty);
                bool isAdmin = options.Value.Admins.Contains(ulong.Parse(cleanSteamId)); // Not worried about error checking here, the SteamAPI "should" return a ulong
                return Ok(new SteamIdResponse()
                {
                    SteamId = cleanSteamId,
                    Name = User.FindFirstValue(ClaimTypes.Name) ?? (isAdmin ? "Admin" : "User"),
                    IsAdmin = isAdmin,
                });
            }

            return NotFound();
        }
    }
}
