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
        public async Task<IActionResult> Edit([FromBody]EditMeasurementsModel measurements)
        {
            _context.Measurements.Update(measurements.ToMeasurements());
            await _context.SaveChangesAsync();
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

        // Return a list of objects containing data about the ingredients over the last (period) time split into (interval) intervals
        [HttpGet]
        public async Task<IActionResult> TimeSeriesData([FromQuery] string period, [FromQuery] string interval)
        {
            // Helper to parse period/interval strings like "1d", "12h", "30m"
            TimeSpan ParseTimeSpan(string input)
            {
                if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Value cannot be null or empty.");
                input = input.Trim().ToLower();
                if (input.EndsWith("d"))
                    return TimeSpan.FromDays(double.Parse(input[..^1]));
                if (input.EndsWith("h"))
                    return TimeSpan.FromHours(double.Parse(input[..^1]));
                if (input.EndsWith("m"))
                    return TimeSpan.FromMinutes(double.Parse(input[..^1]));
                throw new ArgumentException($"Invalid time format: {input}");
            }

            TimeSpan periodSpan, intervalSpan;
            try
            {
                periodSpan = ParseTimeSpan(period);
                intervalSpan = ParseTimeSpan(interval);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Invalid period or interval: {ex.Message}" });
            }
            if (intervalSpan <= TimeSpan.Zero || periodSpan <= TimeSpan.Zero || intervalSpan > periodSpan)
                return BadRequest(new { Message = "Interval and period must be positive, and interval must not exceed period." });

            var periodStart = DateTime.UtcNow - periodSpan;
            var periodEnd = DateTime.UtcNow;

            var results = await _context.Measurements
                .Include(m => m.Ingredient)
                .Where(m => (m.Ingredient.Name == "Instant Coffee" || m.Ingredient.Name == "Water") && m.Time >= periodStart && m.Time <= periodEnd)
                .ToListAsync();

            var resultsWater = results.Where(m => m.Ingredient.Name == "Water")
                .OrderBy(m => m.Time)
                .Select(m => new { m.Time, m.Value })
                .ToList();

            var resultsCoffee = results.Where(m => m.Ingredient.Name == "Instant Coffee")
                .OrderBy(m => m.Time)
                .Select(m => new { m.Time, m.Value })
                .ToList();

            List<object> series = new();
            int intervals = (int)Math.Ceiling(periodSpan.TotalSeconds / intervalSpan.TotalSeconds);
            for (int i = 0; i < intervals; i++)
            {
                var intervalStart = periodEnd - intervalSpan * (i + 1);
                var intervalEnd = periodEnd - intervalSpan * i;
                var waterValue = resultsWater
                    .Where(m => m.Time >= intervalStart && m.Time < intervalEnd)
                    .Select(m => m.Value)
                    .DefaultIfEmpty(0)
                    .Average();
                var coffeeValue = resultsCoffee
                    .Where(m => m.Time >= intervalStart && m.Time < intervalEnd)
                    .Select(m => m.Value)
                    .DefaultIfEmpty(0)
                    .Average();
                series.Add(new
                {
                    Time = intervalStart,
                    Water = Math.Round(waterValue),
                    Coffee = Math.Round(coffeeValue)
                });
            }
            // Reverse to chronological order
            series.Reverse();
            return Ok(series);
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
