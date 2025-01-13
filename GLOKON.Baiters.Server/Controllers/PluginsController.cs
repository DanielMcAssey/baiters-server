using GLOKON.Baiters.Core.Plugins;
using GLOKON.Baiters.Server.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/plugins")]
    [ApiController]
    [Authorize(Policy = "SteamAdmin")]
    public class PluginsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(PluginLoader.Plugins.Select(plugin => new PluginResponse(plugin)).ToList());
        }
    }
}
