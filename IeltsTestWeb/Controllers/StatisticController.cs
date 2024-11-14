using IeltsTestWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IeltsTestWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class StatisticController : ControllerBase
    {
        private readonly ieltsDbContext database;
        public StatisticController(ieltsDbContext database)
        {
            this.database = database;
        }

        /// <summary>
        /// Get the total number of accounts in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("User")]
        public async Task<IActionResult> GetUserCount()
        {
            var count = await database.Accounts.CountAsync();
            return Ok(new { userCount = count});
        }

        /// <summary>
        /// Get the total number of tests in the system.
        /// </summary>
        [HttpGet("Test")]
        public async Task<IActionResult> GetTestCount()
        {
            var count = await database.Tests.CountAsync();
            return Ok(new { testCount = count });
        }

        /// <summary>
        /// Determine how many times the test has been completed within a specific time frame.
        /// </summary>
        [HttpGet("Attend/{time}")]
        public async Task<IActionResult> GetTestTaken(string time,[FromQuery] int? order)
        {
            if (!ValidTimeFrame(time))
                return BadRequest("Time frame must be one of the followings: week, month, year");

            var count = 0;

            switch(time)
            {
                case "week":

                    if (order == null)
                        order = GetWeekOfMonth(DateTime.Now);

                    if (order > 5 || order < 1)
                        return BadRequest("Invalid week order.");

                    foreach(var result in database.Results)
                        if (result.TestAccess == "public" && GetWeekOfMonth(result.DateMake) == order)
                            count++;

                    break;
                case "month":

                    if (order == null)
                        order = DateTime.Now.Month;

                    if (order > 12 || order < 1)
                        return BadRequest("Invalid month order.");

                    count = await database.Results.CountAsync(result => result.TestAccess == "public" && result.DateMake.Month == order);

                    break;
                case "year":

                    if (order == null)
                        order = DateTime.Now.Year;

                    if (order > DateTime.Now.Year || order < 2024)
                        return BadRequest("Invalide year order.");

                    count = await database.Results.CountAsync(result => result.TestAccess == "public" && result.DateMake.Year == order);

                    break;
            }

            return Ok(new { numberTestTaken = count });
        }
        private static int GetWeekOfMonth(DateTime date)
        {
            DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

            int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            int offset = (dayOfWeek == 0 ? 7 : dayOfWeek) - 1;

            return (date.Day + offset) / 7 + 1;
        }
        private static bool ValidTimeFrame(string timeFrame)
        {
            var acceptTimeTags = new List<string> { "week", "month", "year" };
            return acceptTimeTags.Contains(timeFrame);
        }

        /// <summary>
        /// Determine how many tests that the user has completed within a specific time frame.
        /// </summary>
        [HttpGet("User/{id}")]
        public async Task<IActionResult> GetUserTestTaken(int id, [FromQuery] string? time, [FromQuery] int? order)
        {
            if (await database.Accounts.FindAsync(id) == null)
                return NotFound("Can't find account with id " + id);

            if(time == null)
            {
                var results = await database.Results.CountAsync(result => result.AccountId == id);
                return Ok(new { testTakenCount = results });
            }

            if (!ValidTimeFrame(time))
                return BadRequest("Time frame must be one of the followings: week, month, year");

            var count = 0;

            switch (time)
            {
                case "week":

                    if (order == null)
                        order = GetWeekOfMonth(DateTime.Now);

                    if (order > 5 || order < 1)
                        return BadRequest("Invalid week order.");

                    foreach (var result in database.Results)
                        if (result.AccountId == id && GetWeekOfMonth(result.DateMake) == order)
                            count++;

                    break;
                case "month":

                    if (order == null)
                        order = DateTime.Now.Month;

                    if (order > 12 || order < 1)
                        return BadRequest("Invalid month order.");

                    count = await database.Results.CountAsync(result => result.AccountId == id && result.DateMake.Month == order);

                    break;
                case "year":

                    if (order == null)
                        order = DateTime.Now.Year;

                    if (order > DateTime.Now.Year || order < 2024)
                        return BadRequest("Invalide year order.");

                    count = await database.Results.CountAsync(result => result.AccountId == id && result.DateMake.Year == order);

                    break;
            }

            return Ok(new { numberTestTaken = count });
        }

        /// <summary>
        /// Determine how many hours that the user has spent on tests within a specific time frame.
        /// </summary>
        [HttpGet("Time/{id}")]
        public async Task<IActionResult> GetUserTotalTimeSpend(int id, [FromQuery] string? time, [FromQuery] int? order)
        {
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet("Score/{id}")]
        public async Task<IActionResult> GetUserAvgScore(int id, [FromQuery] string? time, [FromQuery] int? order)
        {
            return Ok();
        }

        //Pho diem
    }
}
