using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.DomainModels;
using KaffeMaskineProjekt.Repository;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OrdersController : Controller
    {
        private readonly KaffeDBContext _context;

        private bool IsCurrentlyHandlingOrder = false;

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

        //Gets all orders that have not been served
        [HttpGet]
        public async Task<IActionResult> Unserved()
        {
            var unservedOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Recipe)
                .Where(o => !o.HasBeenServed)
                .AsNoTracking()
                .ToListAsync();
            return Ok(unservedOrders);
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
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateOrderModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get the current user from the JWT token
            var recipe = await _context.Recipes.FindAsync(model.RecipeId);

            var jwtsub = User.FindFirst("userid")?.Value;

            if (int.TryParse(jwtsub, out int userId) == false)
                return BadRequest("Invalid UserId.");

            var user = await _context.Users.FindAsync(userId);

            if (user is null || recipe is null)
                return BadRequest("Invalid UserId or RecipeId, Try again.");

            var exists = await _context.Orders.AnyAsync(x => x.User.Id == userId && x.Recipe.Id == recipe.Id && !x.HasBeenServed);

            if (exists)
                return Conflict("The order you are trying to make already exists.");

            var newOrder = model.ToOrder();
            newOrder.User = user;
            newOrder.Recipe = recipe;

            // Add the new order to the context
            _context.Orders.Add(newOrder);

            // Handle statistics update
            var statistics = await _context.Statistics
                .FirstOrDefaultAsync(s => s.Recipe.Id == recipe.Id && s.User.Id == userId);
            if (statistics is null)
                {
                statistics = new Statistics
                {
                    Recipe = recipe,
                    User = user,
                    NumberOfUses = 1
                };
                _context.Statistics.Add(statistics);
            }
            else
            {
                statistics.NumberOfUses++;
                _context.Statistics.Update(statistics);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            var createdOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Recipe)
                .FirstOrDefaultAsync(o => o.Id == newOrder.Id);

            return CreatedAtAction(nameof(Details), new { id = newOrder.Id }, createdOrder);
        }




        //allows you to edit an order
        [HttpPut]
        [Authorize]
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
        [Authorize]
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

        [HttpPost]
        public async Task<IActionResult> SetBusy([FromBody] bool isBusy)
        {
            IsCurrentlyHandlingOrder = isBusy;
            return Ok(new { IsBusy = IsCurrentlyHandlingOrder });
        }

        [HttpGet]
        public async Task<IActionResult> IsBusy()
        {
            return Ok(new { IsBusy = IsCurrentlyHandlingOrder });
        }

        [HttpGet]
        public async Task<IActionResult> FirstOrderIfBusy()
        {
            if (!IsCurrentlyHandlingOrder)
            {
                return NotFound(new { Message = "No orders being handled." });
            }

            var firstOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Recipe)
                .OrderBy(o => o.Id)
                .FirstOrDefaultAsync();

            if (firstOrder == null)
            {
                return NotFound(new { Message = "No orders found." });
            }

            return Ok(firstOrder);
        }

        [HttpPut]
        public async Task<IActionResult> MarkAsServed()
        {
            if (!IsCurrentlyHandlingOrder)
            {
                return NotFound(new { Message = "No orders being handled." });
            }

            var firstOrder = await _context.Orders
                .OrderBy(o => o.Id)
                .FirstOrDefaultAsync();

            firstOrder.HasBeenServed = true;
            _context.Orders.Update(firstOrder);
            await _context.SaveChangesAsync();
            return Ok(firstOrder);
        }
    }

    public class CreateOrderModel
    {
        public int RecipeId { get; set; }

        public Order ToOrder()
        {
            return new Order
            {
                RecipeId = RecipeId,
                HasBeenServed = false
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
