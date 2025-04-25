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
            return Ok(await _context.Statistics.ToListAsync());
        }

        // GET details
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var statistics = await _context.Statistics.FindAsync(id);
            if (statistics == null)
            {
                return NotFound();
            }

            return Ok(statistics);
        }

        // POST: Measurements
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateStatisticsModel statistics)
        {
            _context.Statistics.Add(statistics.ToStatistics());
            _context.SaveChanges();
            return Ok(statistics);
        }

        // PUT: Measurements
        [HttpPut]
        public IActionResult Update([FromBody] EditStatisticsModel statistics)
        {
            _context.Statistics.Update(statistics.ToStatistics());
            _context.SaveChanges();
            return Ok(statistics);
        }

        // DELETE: Measurements
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var statistics = await _context.Statistics.FindAsync(id);
            _context.Statistics.Remove(statistics);
            _context.SaveChanges();
            return Ok(statistics);
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
