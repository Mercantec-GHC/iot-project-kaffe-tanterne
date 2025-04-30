using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.DomainModels;
using KaffeMaskineProjekt.Repository;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OrdersController : Controller
    {
        private readonly KaffeDBContext _context;

        public OrdersController(KaffeDBContext context)
        {
            _context = context;
        }

        //gets all orders
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Recipe)
                .ToListAsync();

            return Ok(orders);
        }

        //gets a specific order
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        //allows you to create an order
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(model.UserId);
            var recipe = await _context.Recipes.FindAsync(model.RecipeId);

            if (user == null || recipe == null)
                return BadRequest("Invalid UserId or RecipeId, Try again.");

            var exists = await _context.Orders.AnyAsync(x => x.User.Id == user.Id && x.Recipe.Id == recipe.Id && !x.HasBeenServed);

            if (exists)
                return Conflict("The order you are trying to make already exists.");

            var newOrder = model.ToOrder();
            newOrder.User = user;
            newOrder.Recipe = recipe;

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            var createdOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Recipe)
                .FirstOrDefaultAsync(o => o.Id == newOrder.Id);

            return CreatedAtAction(nameof(Details), new { id = newOrder.Id }, createdOrder);
        }




        //allows you to edit an order
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EditOrderModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(model.UserId);
            var recipe = await _context.Recipes.FindAsync(model.RecipeId);

            if (user == null || recipe == null)
                return BadRequest("Invalid UserId or RecipeId.");

            var updatedOrder = model.ToOrder(user, recipe);
            _context.Orders.Update(updatedOrder);
            await _context.SaveChangesAsync();

            return Ok(updatedOrder);
        }

        //allows you to delete a selected order
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return Ok(order);
        }
    }
    public class CreateOrderModel
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public bool HasBeenServed { get; set; }

        public Order ToOrder()
        {
            return new Order
            {
                UserId = UserId,
                RecipeId = RecipeId,
                HasBeenServed = HasBeenServed
            };
        }
    }
    public class EditOrderModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public bool HasBeenServed { get; set; }

        public Order ToOrder(User user, Recipe recipe)
        {
            return new Order
            {
                Id = Id,
                User = user,
                Recipe = recipe,
                HasBeenServed = HasBeenServed
            };
        }
    }
}
