using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        [HttpPost()]
        public IActionResult GetResource([FromBody] string path)
        {
            var url = $"{Request.Scheme}://{Request.Host}{path}";
            return Ok(url);
        }
    }
}
