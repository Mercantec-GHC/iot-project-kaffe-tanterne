using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.Repository;
using KaffeMaskineProjekt.DomainModels;
using System.Threading.Tasks;

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
            var measurements = await _context.Measurements
                .Include(m => m.Ingredient)
                .ToListAsync();

            return Ok(measurements);
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
        public async Task<IActionResult> Create([FromBody]CreateMeasurementsModel measurements)
        {
            var ingredient = await _context.Ingredients.FindAsync(measurements.IngredientId);

            var newMeasurement = measurements.ToMeasurements(ingredient);
            _context.Measurements.Add(newMeasurement);
            await _context.SaveChangesAsync();
            return Ok(measurements);
        }

        // PUT: Measurements
        [HttpPut]
        public IActionResult Edit([FromBody]EditMeasurementsModel measurements)
        {
            _context.Measurements.Update(measurements.ToMeasurements());
            _context.SaveChanges();
            return Ok(measurements);
        }

        // DELETE: Measurements
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var measurements = await _context.Measurements.FindAsync(id);
            if (measurements == null)
            {
                return NotFound(new { Message = "Measurement not found." });
            }

            _context.Measurements.Remove(measurements);
            await _context.SaveChangesAsync(); // Ensure SaveChangesAsync is awaited
            return Ok(new { Message = "Measurement deleted successfully." });
        }


        [HttpGet]
        public async Task<IActionResult> WaterLevel()
        {
            // Return in percentage the current water level
            var totalWater = (await _context.Measurements
                .Where(m => m.Ingredient.Name == "Water")
                .OrderByDescending(m => m.Time)
                .FirstOrDefaultAsync())?.Value ?? 0;

            var maxWater = 1600; // Assuming the maximum water level is 1600g

            double waterLevelPercentage = (totalWater / (double)maxWater) * 100;

            // Ensure the percentage is between 0 and 100
            waterLevelPercentage = Math.Clamp(waterLevelPercentage, 0, 100);

            // Return the water level percentage
            int waterLevelPercentageRounded = (int)Math.Round(waterLevelPercentage, 0);

            return Ok(waterLevelPercentageRounded);
        }

        [HttpGet]
        public async Task<IActionResult> CoffeeLevel()
        {
            // Return in percentage the current coffee level
            var totalCoffee = (await _context.Measurements
                .Where(m => m.Ingredient.Name == "Instant Coffee")
                .OrderByDescending(m => m.Time)
                .FirstOrDefaultAsync())?.Value ?? 0;
            var maxCoffee = 50; // Assuming the maximum coffee level is 50g
            double coffeeLevelPercentage = (totalCoffee / (double)maxCoffee) * 100;

            // Ensure the percentage is between 0 and 100
            coffeeLevelPercentage = Math.Clamp(coffeeLevelPercentage, 0, 100);
            
            // Return the coffee level percentage
            int coffeeLevelPercentageRounded = (int)Math.Round(coffeeLevelPercentage, 0);
            return Ok(coffeeLevelPercentageRounded);
        }

        public class CreateMeasurementsModel
        {
            public required int Value { get; set; }
            public required int IngredientId { get; set; }

            public Measurements ToMeasurements(Ingredient ingredient)
            {
                return new Measurements 
                {
                    Value = Value,
                    Ingredient = ingredient
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
