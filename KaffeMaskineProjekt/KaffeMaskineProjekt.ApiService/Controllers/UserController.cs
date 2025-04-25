using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.DomainModels;
using KaffeMaskineProjekt.Repository;

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
        public async Task<IActionResult> Create([FromBody] CreateUserModel user)
        {
            if (ModelState.IsValid && !(await _context.Users.AnyAsync(x => x.Name.ToLower().Trim() == user.Name.ToLower().Trim() && x.Password == user.Password)))
            {
                _context.Users.Add(user.ToUser());
                await _context.SaveChangesAsync();

                // Return the created ingredient with its ID
                var createdUser = await _context.Users
                    .Where(x => x.Name.ToLower().Trim() == user.Name.ToLower().Trim())
                    .FirstOrDefaultAsync();

                if (createdUser != null)
                    return CreatedAtAction(nameof(Details), new { id = createdUser.Id }, createdUser);
                else
                    return NotFound();
            }
            return BadRequest(ModelState);
        }

        //allows you to edit an user
        [HttpPut]
        public async Task<IActionResult> Edit(int id, [FromBody][Bind("id")] EditUserModel user)
        {
            if (ModelState.IsValid && !(await _context.Users.AnyAsync(x => x.Name.ToLower().Trim() == user.Name.ToLower().Trim() && x.Id != id && x.Password == user.Password)))
            {
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }
                existingUser.Name = user.Name;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            return BadRequest(ModelState);
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
    public class CreateUserModel
    {
        public required string Name { get; set; }
        public required string Password { get; set; }

        public User ToUser()
        {
            return new User
            {
                Name = Name,
                Password = Password
            };
        }
    }
    public class EditUserModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }
        public User ToUser()
        {
            return new User
            {
                Id = Id,
                Name = Name,
                Password = Password
            };
        }

    }
}
