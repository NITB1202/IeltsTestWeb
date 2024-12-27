using IeltsTestWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IeltsTestWeb.Controllers
{
    [Route("constant")]
    [ApiController]
    [Produces("application/json")]
    public class ConstantController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public ConstantController(ieltsDbContext database)
        {
            this.database = database;
        }

        /// <summary>
        /// Get all constants in the system.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Constant>>> GetAllConstants()
        {
            var constants = await database.Constants.ToListAsync();
            return Ok(constants);
        }

        /// <summary>
        /// Get the constant's value.
        /// </summary>
        [HttpGet("{name}")]
        public async Task<IActionResult> GetConstant(string name)
        {
            var constant = await database.Constants.FindAsync(name);

            if (constant == null)
                return NotFound("Can't find constant with name " + name);

            return Ok(constant);
        }
        
        /// <summary>
        /// Update constant's value.
        /// </summary>
        [HttpPatch("{name}")]
        public async Task<IActionResult> UpdateConstant(string name, [FromBody] decimal value)
        {
            var constant = await database.Constants.FindAsync(name);

            if (constant == null)
                return NotFound("Can't find constant with name " + name);

            constant.Value = value;

            await database.SaveChangesAsync();

            return Ok(constant);

        }
    }
}
