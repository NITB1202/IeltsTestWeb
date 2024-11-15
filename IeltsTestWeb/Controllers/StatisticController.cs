using IeltsTestWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IeltsTestWeb.Utils;

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
        private static int GetWeekOfMonth(DateTime date)
        {
            DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

            int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            int offset = (dayOfWeek == 0 ? 7 : dayOfWeek) - 1;

            return (date.Day + offset) / 7 + 1;
        }
        private static bool ValidWeekOrder(ref int? order)
        {
            if (order == null)
                order = GetWeekOfMonth(DateTime.Now);

            if (order > 5 || order < 1)
                return false;

            return true;
        }
        private static bool ValidMonthOrder(ref int? order)
        {
            if (order == null)
                order = DateTime.Now.Month;

            if (order > 12 || order < 1)
                return false;

            return true;
        }
        private static bool ValidYearOrder(ref int? order)
        {
            if (order == null)
                order = DateTime.Now.Year;

            if (order > DateTime.Now.Year || order < 2024)
                return false;

            return true;
        }
        private static bool ValidTimeFrame(string timeFrame)
        {
            var acceptTimeTags = new List<string> { "week", "month", "year" };
            return acceptTimeTags.Contains(timeFrame);
        }

        /// <summary>
        /// Get the total number of accounts in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("User")]
        public async Task<IActionResult> GetUserCount()
        {
            var count = await database.Accounts.CountAsync();
            return Ok(new { userCount = count });
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
        public async Task<IActionResult> GetTestTaken(string time, [FromQuery] int? order)
        {
            if (!ValidTimeFrame(time))
                return BadRequest("Time frame must be one of the followings: week, month, year");

            var count = 0;

            switch (time)
            {
                case "week":

                    if (!ValidWeekOrder(ref order))
                        return BadRequest("Invalid week order.");

                    foreach (var result in database.Results)
                        if (result.TestAccess == "public" && GetWeekOfMonth(result.DateMake) == order)
                            count++;

                    break;

                case "month":

                    if (!ValidMonthOrder(ref order))
                        return BadRequest("Invalid month order.");

                    count = await database.Results.CountAsync(result => result.TestAccess == "public" && result.DateMake.Month == order);

                    break;

                case "year":

                    if (!ValidYearOrder(ref order))
                        return BadRequest("Invalide year order.");

                    count = await database.Results.CountAsync(result => result.TestAccess == "public" && result.DateMake.Year == order);

                    break;
            }

            return Ok(new { numberTestTaken = count });
        }

        /// <summary>
        /// Determine how many tests that the user has completed within a specific time frame.
        /// </summary>
        [HttpGet("User/{id}")]
        public async Task<IActionResult> GetUserTestTaken(int id, [FromQuery] string? time, [FromQuery] int? order)
        {
            if (await database.Accounts.FindAsync(id) == null)
                return NotFound("Can't find account with id " + id);

            if (!ValidTimeFrame(time))
                return BadRequest("Time frame must be one of the followings: week, month, year");

            var count = 0;

            switch (time)
            {
                case "week":

                    if (!ValidWeekOrder(ref order))
                        return BadRequest("Invalid week order.");

                    foreach (var result in database.Results)
                        if (result.AccountId == id && GetWeekOfMonth(result.DateMake) == order)
                            count++;

                    break;

                case "month":

                    if (!ValidMonthOrder(ref order))
                        return BadRequest("Invalid month order.");

                    count = await database.Results.CountAsync(result => result.AccountId == id && result.DateMake.Month == order);

                    break;

                case "year":

                    if (!ValidYearOrder(ref order))
                        return BadRequest("Invalide year order.");

                    count = await database.Results.CountAsync(result => result.AccountId == id && result.DateMake.Year == order);

                    break;

                default:

                    count = await database.Results.CountAsync(result => result.AccountId == id);
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
            if (await database.Accounts.FindAsync(id) == null)
                return NotFound("Can't find account with id " + id);

            if (time != null && !ValidTimeFrame(time))
                return BadRequest("Time frame must be one of the followings: week, month, year");

            var totalTime = TimeSpan.Zero;

            switch(time)
            {
                case "week":

                    if (!ValidWeekOrder(ref order))
                        return BadRequest("Invalid week order.");

                    foreach(var result in database.Results)
                    {
                        if (result.AccountId == id && GetWeekOfMonth(result.DateMake) == order)
                            totalTime += result.CompleteTime.ToTimeSpan();
                    }

                    break;
                case "month":

                    if (!ValidMonthOrder(ref order))
                        return BadRequest("Invalid month order.");

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id && result.DateMake.Month == order)
                            totalTime += result.CompleteTime.ToTimeSpan();
                    }

                    break;
                case "year":

                    if (!ValidYearOrder(ref order))
                        return BadRequest("Invalide year order.");

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id && result.DateMake.Year == order)
                            totalTime += result.CompleteTime.ToTimeSpan();
                    }

                    break;
                default:

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id)
                            totalTime += result.CompleteTime.ToTimeSpan();
                    }

                    break;
            }

            return Ok(new {totalTime = totalTime});
        }

        /// <summary>
        /// Get user's average band within a specific time frame.
        /// </summary>
        [HttpGet("Score/{id}")]
        public async Task<IActionResult> GetUserAvgScore(int id, [FromQuery] string? time, [FromQuery] int? order)
        {
            if (await database.Accounts.FindAsync(id) == null)
                return NotFound("Can't find account with id " + id);

            if (time != null && !ValidTimeFrame(time))
                return BadRequest("Time frame must be one of the followings: week, month, year");

            var totalScore = 0;
            var totalTest = 0;

            switch (time)
            {
                case "week":

                    if (!ValidWeekOrder(ref order))
                        return BadRequest("Invalid week order.");

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id && GetWeekOfMonth(result.DateMake) == order)
                        {
                            totalScore += result.Score;
                            totalTest++;
                        }
                    }

                    break;
                case "month":

                    if (!ValidMonthOrder(ref order))
                        return BadRequest("Invalid month order.");

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id && result.DateMake.Month == order)
                        {
                            totalScore += result.Score;
                            totalTest++;
                        }
                    }

                    break;
                case "year":

                    if (!ValidYearOrder(ref order))
                        return BadRequest("Invalide year order.");

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id && result.DateMake.Year == order)
                        {
                            totalScore += result.Score;
                            totalTest++;
                        }
                    }

                    break;
                default:

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id)
                        {
                            totalScore += result.Score;
                            totalTest++;
                        }
                    }

                    break;
            }

            var avgBand = ResourcesManager.ConvertScoreToBand(totalScore / totalTest);

            return Ok(new {avgBand = avgBand});
        }

        /// <summary>
        /// Get user's band map within a specific time frame.
        /// </summary>
        [HttpGet("Band/{id}")]
        public async Task<IActionResult> GetUserBandDiagram(int id, [FromQuery] string? time, [FromQuery] int? order)
        {
            if (await database.Accounts.FindAsync(id) == null)
                return NotFound("Can't find account with id " + id);

            if (time != null && !ValidTimeFrame(time))
                return BadRequest("Time frame must be one of the followings: week, month, year");

            var scoreBand = new Dictionary<double, int>();

            for (double value = 1; value <= 9; value += 0.5)
                scoreBand[value] = 0;

            switch (time)
            {
                case "week":

                    if (!ValidWeekOrder(ref order))
                        return BadRequest("Invalid week order.");

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id && GetWeekOfMonth(result.DateMake) == order)
                        {
                            var band = ResourcesManager.ConvertScoreToBand(result.Score);
                            scoreBand[band]++;
                        }
                    }

                    break;
                case "month":

                    if (!ValidMonthOrder(ref order))
                        return BadRequest("Invalid month order.");

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id && result.DateMake.Month == order)
                        {
                            var band = ResourcesManager.ConvertScoreToBand(result.Score);
                            scoreBand[band]++;
                        }
                    }

                    break;
                case "year":

                    if (!ValidYearOrder(ref order))
                        return BadRequest("Invalide year order.");

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id && result.DateMake.Year == order)
                        {
                            var band = ResourcesManager.ConvertScoreToBand(result.Score);
                            scoreBand[band]++;
                        }
                    }

                    break;
                default:

                    foreach (var result in database.Results)
                    {
                        if (result.AccountId == id)
                        {
                            var band = ResourcesManager.ConvertScoreToBand(result.Score);
                            scoreBand[band]++;
                        }
                    }

                    break;
            }

            return Ok(scoreBand);
        }
    }
}