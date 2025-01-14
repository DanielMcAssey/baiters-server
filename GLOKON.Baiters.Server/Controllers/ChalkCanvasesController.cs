using GLOKON.Baiters.Core;
using GLOKON.Baiters.Server.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLOKON.Baiters.Server.Controllers
{
    [Route("api/chalk-canvases")]
    [ApiController]
    [Authorize(Policy = "SteamAdmin")]
    public class ChalkCanvasesController(GameManager gm) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(gm.Server.ChalkCanvases.Select((canvas) => new ChalkCanvasResponse(canvas.Key, canvas.Value)).ToList());
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] long id)
        {
            gm.Server.RemoveChalkCanvas(id);
            return NoContent();
        }
    }
}
