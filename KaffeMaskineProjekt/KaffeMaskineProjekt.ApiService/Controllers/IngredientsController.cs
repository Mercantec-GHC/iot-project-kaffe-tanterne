using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProject.Repository;
using KaffeMaskineProject.DomainModels;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IngredientsController : Controller
    {
        private readonly KaffeDBContext _context;

        public IngredientsController(KaffeDBContext context)
        {
            _context = context;
        }

        // GET: Ingredients
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Ok(await _context.Ingredients.ToListAsync());
        }

        // GET: Ingredients/Details/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            return Ok(ingredient);
        }

        // POST: Ingredients/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIngredientModel ingredient)
        {
            if (ModelState.IsValid && !(await _context.Ingredients.AnyAsync(x => x.Name.ToLower().Trim() == ingredient.Name.ToLower().Trim())))
            {
                _context.Ingredients.Add(ingredient.ToIngredient());
                await _context.SaveChangesAsync();

                // Return the created ingredient with its ID
                var createdIngredient = await _context.Ingredients
                    .Where(x => x.Name.ToLower().Trim() == ingredient.Name.ToLower().Trim())
                    .FirstOrDefaultAsync();

                if (createdIngredient != null)
                    return CreatedAtAction(nameof(Details), new { id = createdIngredient.Id }, createdIngredient);
                else
                    return NotFound();
            }
            return BadRequest(ModelState);
        }

        // PUT: Ingredients/Edit/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody][Bind("id")] EditIngredientModel ingredient)
        {
            if (id != ingredient.Id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid && !(await _context.Ingredients.AnyAsync(x => x.Name.ToLower().Trim() == ingredient.Name.ToLower().Trim() && x.Id != id)))
            {
                var existingIngredient = await _context.Ingredients.FindAsync(id);
                if (existingIngredient == null)
                {
                    return NotFound();
                }
                existingIngredient.Name = ingredient.Name;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            return BadRequest(ModelState);
        }

        // DELETE: Ingredients/Delete/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }


    public class CreateIngredientModel
    {
        public required string Name { get; set; }

        public Ingredient ToIngredient()
        {
            return new Ingredient
            {
                Name = Name
            };
        }
    }

    public class EditIngredientModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public Ingredient ToIngredient()
        {
            return new Ingredient
            {
                Id = Id,
                Name = Name
            };
        }

    }
}
