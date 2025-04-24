using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProject.Repository;
using KaffeMaskineProject.DomainModels;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MeasurementsController : Controller
    {
        private readonly KaffeDBContext _context;

        public MeasurementsController(KaffeDBContext context)
        {
            _context = context;
        }

        // GET: Measurements
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Ok(await _context.Measurements.ToListAsync());
        }

        // GET details
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var measurements = await _context.Measurements
                .FirstOrDefaultAsync(m => m.Id == id);
            if (measurements == null)
            {
                return NotFound();
            }

            return Ok(measurements);
        }

        // POST: Measurements
        [HttpPost]
        public IActionResult Create([FromBody]Measurements measurements)
        {
            _context.Measurements.Add(measurements);
            _context.SaveChanges();
            return Ok(measurements);
        }

        // PUT: Measurements
        [HttpPut]
        public IActionResult Update([FromBody]Measurements measurements)
        {
            _context.Measurements.Update(measurements);
            _context.SaveChanges();
            return Ok(measurements);
        }

        // DELETE: Measurements
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var measurements = await _context.Measurements.FindAsync(id);
            _context.Measurements.Remove(measurements);
            _context.SaveChanges();
            return Ok(measurements);
        }
    }
}
