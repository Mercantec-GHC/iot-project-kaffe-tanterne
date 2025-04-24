using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProject.Repository;
using KaffeMaskineProject.DomainModels;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RecipeController : Controller
    {
        private readonly KaffeDBContext _context;

        public RecipeController(KaffeDBContext context)
        {
            _context = context;
        }

        // GET: Recipes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var recipes = await _context.Recipes
                .Include(r => r.IngredientRecipes)
                .ThenInclude(ri => ri.Ingredient)
                .ToListAsync();
            var result = recipes.Select(r => new ViewRecipeModel
            {
                Id = r.Id,
                Name = r.Name,
                Ingredients = r.IngredientRecipes.ToDictionary(ri => ri.IngredientId, ri => ri.Amount)
            });
            return Ok(result);
        }

        // GET: Recipes/Details/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.IngredientRecipes)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
                return NotFound();
            var viewModel = new ViewRecipeModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Ingredients = recipe.IngredientRecipes.ToDictionary(ri => ri.IngredientId, ri => ri.Amount)
            };
            return Ok(viewModel);
        }

        // POST: Recipes/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecipeModel model)
        {
            var recipe = model.ToRecipe();
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            var viewModel = new ViewRecipeModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Ingredients = recipe.IngredientRecipes.ToDictionary(ri => ri.IngredientId, ri => ri.Amount)
            };
            return CreatedAtAction(nameof(Details), new { id = recipe.Id }, viewModel);
        }

        // PUT: Recipes/Edit/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] EditRecipeModel model)
        {
            if (id != model.Id)
                return BadRequest();
            var existingRecipe = await _context.Recipes
                .Include(r => r.IngredientRecipes)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (existingRecipe == null)
                return NotFound();

            existingRecipe.Name = model.Name;
            _context.RecipeIngredients.RemoveRange(existingRecipe.IngredientRecipes);
            existingRecipe.IngredientRecipes = model.Ingredients.Select(i => new RecipeIngredient
            {
                RecipeId = id,
                IngredientId = i.Key,
                Amount = i.Value
            }).ToList();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: Recipes/Delete/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.IngredientRecipes)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
                return NotFound();

            _context.RecipeIngredients.RemoveRange(recipe.IngredientRecipes);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CreateRecipeModel
    {
        public required string Name { get; set; }
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();

        public Recipe ToRecipe()
        {
            return new Recipe
            {
                Name = Name,
                IngredientRecipes = Ingredients.Select(ingredient => new RecipeIngredient
                {
                    IngredientId = ingredient.Key,
                    Amount = ingredient.Value
                }).ToList()
            };
        }
    }

    public class EditRecipeModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();
    }

    public class ViewRecipeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();
    }
}
