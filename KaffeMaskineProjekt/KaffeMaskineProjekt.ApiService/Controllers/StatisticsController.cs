using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.Repository;
using KaffeMaskineProjekt.DomainModels;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StatisticsController : Controller
    {
        private readonly KaffeDBContext _context;

        public StatisticsController(KaffeDBContext context)
        {
            _context = context;
        }

        // GET: Statistics
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Fetch the sum of NumberOfUses ordered by recipe name across all users
            var statistics = await _context.Statistics
                .Include(s => s.Recipe)
                .GroupBy(s => s.Recipe.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    TotalNumberOfUses = g.Sum(s => s.NumberOfUses)
                })
                .AsNoTracking()
                .ToListAsync();

            if (statistics == null)
            {
                return NotFound();
            }

            return Ok(statistics);
        }

        // GET details
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            // Fetch the sum of NumberOfUses for a specific recipe
            var statistics = await _context.Statistics
                .Include(s => s.Recipe)
                .Include(s => s.User)
                .Where(s => s.Recipe.Id == id)
                .GroupBy(s => s.Recipe.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    TotalNumberOfUses = g.Sum(s => s.NumberOfUses),
                    RecipeId = id,
                    Users = g.Select(s => new { s.User.Name, s.User.Id }).Distinct().ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (statistics == null)
            {
                return NotFound();
            }

            return Ok(statistics);
        }

        // POST: Measurements
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStatisticsModel statistics)
        {
            _context.Statistics.Add(statistics.ToStatistics());
            await _context.SaveChangesAsync();
            return Ok(statistics);
        }

        // PUT: Measurements
        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] EditStatisticsModel statistics)
        {
            _context.Statistics.Update(statistics.ToStatistics());
            await _context.SaveChangesAsync();
            return Ok(statistics);
        }

        // DELETE: Measurements
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var statistics = await _context.Statistics.FindAsync(id);
            _context.Statistics.Remove(statistics);
            await _context.SaveChangesAsync();
            return Ok(statistics);
        }

        [HttpGet]
        public async Task<IActionResult> TotalCoffeesOrdered()
        {
            // Calculate the total number of coffees ordered across all recipes and users
            var totalCoffees = await _context.Statistics
                .SumAsync(s => s.NumberOfUses);

            return Ok(totalCoffees);
        }

        [HttpGet]
        public async Task<IActionResult> TotalUsers()
        {
            // Calculate the total number of unique users
            var totalUsers = await _context.Users
                .CountAsync();

            return Ok(totalUsers);
        }

        [HttpGet]
        public async Task<IActionResult> MostPopularTimes()
        {
            string resp = "8 - 9 PM"; // Placeholder response

            // Fetch the most popular times to order coffee in a readable format like "8-10 AM"
            var popularTimes = await _context.Orders
                .GroupBy(o => new { Hour = o.OrderDate.Hour, Min = (o.OrderDate.Minute / 30) }) // Group by hour and half-hour intervals
                .Select(g => new
                {
                    Time = $"{g.Key.Hour}-{g.Key.Hour + 1} {(g.Key.Min == 0 ? "AM" : "PM")}",
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();

            resp = popularTimes is not null ? popularTimes.Time : resp;

            return Ok(resp);
        }

        public class CreateStatisticsModel
        {
            public required int RecipeId { get; set; }
            public required int UserId { get; set; }
            public required int NumberOfUses { get; set; }

            public Statistics ToStatistics()
            {
                return new Statistics
                {
                    RecipeId = RecipeId,
                    UserId = UserId,
                    NumberOfUses = NumberOfUses
                };
            }
        }
        public class EditStatisticsModel
        {
            public required int Id { get; set; }
            public required int RecipeId { get; set; }
            public required int UserId { get; set; }
            public required int NumberOfUses { get; set; }

            public Statistics ToStatistics()
            {
                return new Statistics
                {
                    Id = Id,
                    RecipeId = RecipeId,
                    UserId = UserId,
                    NumberOfUses = NumberOfUses
                };
            }
        }
    }
}
