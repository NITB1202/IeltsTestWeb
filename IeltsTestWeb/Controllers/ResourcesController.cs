using IeltsTestWeb.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ResourcesController : ControllerBase
    {
        /// <summary>
        ///  Get resource url by relative path
        /// </summary>
        [HttpPost]
        public IActionResult GetResource([FromBody] string path)
        {
            var url = $"{Request.Scheme}://{Request.Host}{path}";
            return Ok(url);
        }

        /// <summary>
        /// Convert score to band.
        /// </summary>
        [HttpGet]
        public IActionResult ScoreToBand([FromQuery] double score)
        {
            return Ok(ResourcesManager.ConvertScoreToBand(score));
        }
    }
}
