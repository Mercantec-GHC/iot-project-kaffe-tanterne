using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProject.DomainModels;
using KaffeMaskineProject.Repository;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly KaffeDBContext _context;

        public UserController(KaffeDBContext context)
        {
            _context = context;
        }

        //gets all users
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        //gets a specific user
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        //allows you to create an user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Post([FromBody] User user)
        {
            _context.Add(user);
            _context.SaveChanges();
            return Ok(user);   
        }

        //allows you to edit an user
        [HttpPut]
        public IActionResult Update([FromBody] User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
            return Ok(user);
        }

        //allows you to delete a selected user
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok(user);
        }
    }
}
