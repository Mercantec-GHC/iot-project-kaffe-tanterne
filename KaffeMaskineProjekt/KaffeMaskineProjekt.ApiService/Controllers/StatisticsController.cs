using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProject.Repository;
using KaffeMaskineProject.DomainModels;
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
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var statistics = await _context.Measurements
                .FirstOrDefaultAsync(m => m.Id == id);
            if (statistics == null)
            {
                return NotFound();
            }

            return Ok(statistics);
        }

        // POST: Measurements
        [HttpPost]
        public IActionResult Post([FromBody] Statistics statistics)
        {
            _context.Statistics.Add(statistics);
            _context.SaveChanges();
            return Ok(statistics);
        }

        // PUT: Measurements
        [HttpPut]
        public IActionResult Update([FromBody] Statistics statistics)
        {
            _context.Statistics.Update(statistics);
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
    }
}
