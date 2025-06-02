using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.DomainModels;
using KaffeMaskineProjekt.Repository;
using KaffeMaskineProjekt.ApiService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly KaffeDBContext _context;
        private readonly TokenService _tokenService;

        public UserController(KaffeDBContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
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
                {
                    return _tokenService.Create(createdUser) switch
                    {
                        string token => Ok(new { Token = token, User = createdUser }),
                        _ => BadRequest("Failed to create token.")
                    };
                }
                else
                    return NotFound();
            }
            return BadRequest(ModelState);
        }

        //allows you to login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (string.IsNullOrWhiteSpace(loginModel.Email) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return BadRequest("Username or password cannot be empty.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginModel.Email && u.Password == loginModel.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = _tokenService.Create(user);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = _tokenService.GenerateRefreshToken(),
                ExpiryDate = DateTime.UtcNow.AddDays(7) // Set the expiry date for the refresh token
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();


            return Ok(new LoginResponseModel() { User = user, AccessToken = token, RefreshToken = refreshToken.Token });
        }

        //allows you to refresh the token
        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            RefreshToken? token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || token.ExpiryDate < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired refresh token.");
            }

            string accessToken = _tokenService.Create(token.User);

            token.Token = _tokenService.GenerateRefreshToken();
            token.ExpiryDate = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new LoginResponseModel() { AccessToken = accessToken, RefreshToken = token.Token, User = token.User });
        }

        //allows you to edit an user
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody][Bind("id")] EditUserModel user)
        {
            if (ModelState.IsValid && !(await _context.Users.AnyAsync(x => x.Name.ToLower().Trim() == user.Name.ToLower().Trim() && x.Id != id && x.Password == user.Password)))
            {
                var existingUser = await _context.Users.FirstAsync(user => user.Id == id);
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
            await _context.SaveChangesAsync();
            return Ok(user);
        }
    }
    public class CreateUserModel
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }

        public User ToUser()
        {
            return new User
            {
                Name = Name,
                Password = Password,
                Email = Email,
                Roles = new List<string>()
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
    public class LoginModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponseModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public User User { get; set; }
    }
}
