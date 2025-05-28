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
                .Where(o => o.HasBeenServed == OrderStatus.Pending)
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

            var exists = await _context.Orders.AnyAsync(x => x.User.Id == userId && x.Recipe.Id == recipe.Id && x.HasBeenServed != OrderStatus.Served);

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

        [HttpGet]
        public async Task<IActionResult> FirstOrderIfBusy()
        {
            if (await _context.Orders.AnyAsync(o => o.HasBeenServed == OrderStatus.Handling) == false)
            {
                return NotFound(new { Message = "No orders being handled." });
            }

            // TEST: Insert a test order if none exist
            if (!await _context.Orders.AnyAsync())
            {
                var testOrder = new Order
                {
                    // Set required properties for your Order entity
                    RecipeId = 1, // Use a valid RecipeId from your DB
                    HasBeenServed = OrderStatus.Handling
                };
                _context.Orders.Add(testOrder);
                await _context.SaveChangesAsync();
            }

            var firstOrder = await _context.Orders
                .Where(o => o.HasBeenServed == OrderStatus.Handling)
                .OrderBy(o => o.Id)
                .Include(o => o.Recipe)
                .FirstOrDefaultAsync();

            if (firstOrder == null)
            {
                return NotFound(new { Message = "No orders found." });
            }

            // Return only the Id (and optionally other fields)
            return Ok(new { Id = firstOrder.Id });
        }

        [HttpGet]
        public async Task<IActionResult> MachineBusy()
        {
            // Check if there are any orders being handled
            var isBusy = await _context.Orders.AnyAsync(o => o.HasBeenServed == OrderStatus.Handling);
            return Ok(new { IsBusy = isBusy });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> MarkAsServed(int id)
        {
            
            if (await _context.Orders.AnyAsync(o => o.HasBeenServed == OrderStatus.Handling) == false)
            {
                return NotFound(new { Message = "No orders being handled." });
            }

            // Find the first unserved order
            var firstOrder = await _context.Orders
                .Where(o => o.HasBeenServed == OrderStatus.Handling)
                .OrderBy(o => o.Id)
                .FirstOrDefaultAsync();

            if (firstOrder == null)
            {
                return NotFound(new { Message = "No unserved orders found." });
            }

            // Only allow marking as served if the id matches the first unserved order
            if (firstOrder.Id != id)
            {
                return BadRequest(new { Message = "You can only mark the first unserved order as served." });
            }

            firstOrder.HasBeenServed = OrderStatus.Served;
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
                HasBeenServed = OrderStatus.Pending
            };
        }
    }
    public class EditOrderModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public OrderStatus HasBeenServed { get; set; }

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
