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
            return Ok(await _context.Orders.ToListAsync());
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
        [ValidateAntiForgeryToken]
        public IActionResult Post([FromBody] Order order)
        {
            _context.Add(order);
            _context.SaveChanges();
            return Ok(order);
        }

        //allows you to edit an order
        [HttpPut]
        public IActionResult Update([FromBody] Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
            return Ok(order);
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
}
