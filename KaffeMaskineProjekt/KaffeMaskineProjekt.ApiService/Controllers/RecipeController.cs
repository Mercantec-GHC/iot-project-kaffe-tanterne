using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.Repository;
using KaffeMaskineProjekt.DomainModels;
using System.ComponentModel;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    /// <summary>
    /// API controller for managing coffee recipes
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RecipeController : Controller
    {
        private readonly KaffeDBContext _context;

        public RecipeController(KaffeDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all available recipes
        /// </summary>
        /// <returns>A list of all recipes with their ingredients</returns>
        /// <response code="200">Returns the list of recipes</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ViewRecipeModel>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Retrieves details for a specific recipe
        /// </summary>
        /// <param name="id">The unique identifier of the recipe</param>
        /// <returns>Detailed information about the requested recipe</returns>
        /// <response code="200">Returns the requested recipe</response>
        /// <response code="404">If the recipe doesn't exist</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ViewRecipeModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Details([Description("The unique identifier of the recipe")]int id)
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

        /// <summary>
        /// Creates a new recipe
        /// </summary>
        /// <param name="model">The recipe data to create</param>
        /// <returns>The newly created recipe with its assigned ID</returns>
        /// <response code="201">Returns the newly created recipe</response>
        /// <response code="400">If the recipe data is invalid or an ingredient doesn't exist</response>
        [HttpPost]
        [ProducesResponseType(typeof(ViewRecipeModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody, Description("The recipe data to create")] CreateRecipeModel model)
        {
            Recipe? recipe = null;

            try
            {
                recipe = model.ToRecipe(_context);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            
            if (recipe == null)
                return BadRequest("Invalid recipe data.");

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            var viewModel = new ViewRecipeModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Ingredients = recipe.IngredientRecipes.Where(ri => ri.RecipeId == recipe.Id).ToDictionary(ri => ri.IngredientId, ri => ri.Amount)
            };
            return CreatedAtAction(nameof(Details), new { id = recipe.Id }, viewModel);
        }

        /// <summary>
        /// Updates an existing recipe
        /// </summary>
        /// <param name="id">The unique identifier of the recipe to update</param>
        /// <param name="model">The updated recipe data</param>
        /// <returns>No content on successful update</returns>
        /// <response code="204">If the recipe was successfully updated</response>
        /// <response code="400">If the ID in the URL doesn't match the ID in the model</response>
        /// <response code="404">If the recipe doesn't exist</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit([Description("The unique identifier of the recipe to update")]int id, [FromBody, Bind("id"), Description("The updated recipe data")] EditRecipeModel model)
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

        /// <summary>
        /// Deletes a specific recipe
        /// </summary>
        /// <param name="id">The unique identifier of the recipe to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">If the recipe was successfully deleted</response>
        /// <response code="404">If the recipe doesn't exist</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([Description("The unique identifier of the recipe to delete")]int id)
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

    /// <summary>
    /// Model for creating a new recipe
    /// </summary>
    public class CreateRecipeModel
    {
        /// <summary>
        /// The name of the recipe
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Dictionary of ingredients with their amounts
        /// Key: Ingredient ID, Value: Amount
        /// </summary>
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Converts the model to a Recipe entity
        /// </summary>
        /// <param name="context">Database context to validate ingredients</param>
        /// <returns>A new Recipe entity</returns>
        /// <exception cref="ArgumentException">Thrown when an ingredient ID doesn't exist</exception>
        public Recipe ToRecipe(KaffeDBContext context)
        {
            var recipe = new Recipe
            {
                Name = Name,
                IngredientRecipes = new List<RecipeIngredient>()
            };

            var allIngredients = context.Ingredients.ToList();

            foreach (var ingredient in Ingredients)
            {
                if (allIngredients.Any(i => i.Id == ingredient.Key))
                {
                    recipe.IngredientRecipes.Add(new RecipeIngredient
                    {
                        IngredientId = ingredient.Key,
                        Amount = ingredient.Value
                    });
                }
                else
                {
                    throw new ArgumentException($"Ingredient with ID {ingredient.Key} does not exist.");
                }
            }

            return recipe;
        }
    }

    /// <summary>
    /// Model for updating an existing recipe
    /// </summary>
    public class EditRecipeModel
    {
        /// <summary>
        /// The unique identifier of the recipe
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The updated name of the recipe
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Dictionary of ingredients with their amounts
        /// Key: Ingredient ID, Value: Amount
        /// </summary>
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();
    }

    /// <summary>
    /// Model for viewing recipe details
    /// </summary>
    public class ViewRecipeModel
    {
        /// <summary>
        /// The unique identifier of the recipe
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the recipe
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Dictionary of ingredients with their amounts
        /// Key: Ingredient ID, Value: Amount
        /// </summary>
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();
    }
}
