using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.Repository;
using KaffeMaskineProjekt.DomainModels;

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
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var measurements = await _context.Measurements.FindAsync(id);
            if (measurements == null) 
            {
                return NotFound();
            }

            return Ok(measurements);
        }

        // POST: Measurements
        [HttpPost]
        public IActionResult Post([FromBody]CreateMeasurementsModel measurements)
        {
            _context.Measurements.Add(measurements.ToMeasurements());
            _context.SaveChangesAsync();
            return Ok(measurements);
        }

        // PUT: Measurements
        [HttpPut]
        public IActionResult Update([FromBody]EditMeasurementsModel measurements)
        {
            _context.Measurements.Update(measurements.ToMeasurements());
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

        public class CreateMeasurementsModel
        {
            public required int Value { get; set; }
            public required int IngredientId { get; set; }

            public Measurements ToMeasurements()
            {
                return new Measurements 
                {
                    Value = Value,
                    IngredientId = IngredientId
                };
            }
        }
        public class EditMeasurementsModel
        {
            public required int Id { get; set; }
            public required int Value { get; set; }
            public required int IngredientId { get; set; }

            public Measurements ToMeasurements()
            {
                return new Measurements
                {
                    Id = Id,
                    Value = Value,
                    IngredientId = IngredientId
                };
            }
        }
    }
}
