using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using KaffeMaskineProjekt.Repository;
using KaffeMaskineProjekt.DomainModels;
using System.ComponentModel;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    /// <summary>
    /// API controller for managing coffee ingredients
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IngredientsController : Controller
    {
        private readonly KaffeDBContext _context;

        public IngredientsController(KaffeDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all available ingredients
        /// </summary>
        /// <returns>A list of all ingredients</returns>
        /// <response code="200">Returns the list of ingredients</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Ingredient>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Index()
        {
            return Ok(await _context.Ingredients.ToListAsync());
        }

        /// <summary>
        /// Retrieves details for a specific ingredient
        /// </summary>
        /// <param name="id">The unique identifier of the ingredient</param>
        /// <returns>Detailed information about the requested ingredient</returns>
        /// <response code="200">Returns the requested ingredient</response>
        /// <response code="404">If the ingredient doesn't exist</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Ingredient), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Details([Description("The unique identifier of the ingredient")]int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            return Ok(ingredient);
        }

        /// <summary>
        /// Creates a new ingredient
        /// </summary>
        /// <param name="ingredient">The ingredient data to create</param>
        /// <returns>The newly created ingredient with its assigned ID</returns>
        /// <response code="201">Returns the newly created ingredient</response>
        /// <response code="400">If the ingredient data is invalid or the name already exists</response>
        [HttpPost]
        [ProducesResponseType(typeof(Ingredient), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody, Description("The ingredient data to create")] CreateIngredientModel ingredient)
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

        /// <summary>
        /// Updates an existing ingredient
        /// </summary>
        /// <param name="id">The unique identifier of the ingredient to update</param>
        /// <param name="ingredient">The updated ingredient data</param>
        /// <returns>No content on successful update</returns>
        /// <response code="204">If the ingredient was successfully updated</response>
        /// <response code="400">If the ID in the URL doesn't match the ID in the model or the name already exists</response>
        /// <response code="404">If the ingredient doesn't exist</response>
        [HttpPut("{id}")]
        [EndpointDescription("Update an existing ingredient")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit([Description("The unique identifier of the ingredient to update")] int id, [FromBody, Bind("id"), Description("The updated ingredient data")] EditIngredientModel ingredient)
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

        /// <summary>
        /// Deletes a specific ingredient
        /// </summary>
        /// <param name="id">The unique identifier of the ingredient to delete</param>
        /// <returns>No content on successful deletion</returns>
        /// <response code="204">If the ingredient was successfully deleted</response>
        /// <response code="404">If the ingredient doesn't exist</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([Description("The unique identifier of the ingredient to delete")]int id)
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

    /// <summary>
    /// Model for creating a new ingredient
    /// </summary>
    public class CreateIngredientModel
    {
        /// <summary>
        /// The name of the ingredient
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Converts the model to an Ingredient entity
        /// </summary>
        /// <returns>A new Ingredient entity</returns>
        public Ingredient ToIngredient()
        {
            return new Ingredient
            {
                Name = Name
            };
        }
    }

    /// <summary>
    /// Model for updating an existing ingredient
    /// </summary>
    public class EditIngredientModel
    {
        /// <summary>
        /// The unique identifier of the ingredient
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The updated name of the ingredient
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Converts the model to an Ingredient entity
        /// </summary>
        /// <returns>An updated Ingredient entity</returns>
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
