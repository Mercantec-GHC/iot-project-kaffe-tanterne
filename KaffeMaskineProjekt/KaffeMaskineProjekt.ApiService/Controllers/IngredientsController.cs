using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProject.Repository;

namespace KaffeMaskineProjekt.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IngredientsController : Controller
    {
        private readonly KaffeDBContext _context;

        public IngredientsController(KaffeDBContext context)
        {
            _context = context;
        }

        // GET: Ingredients
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Ok(await _context.Ingredients.ToListAsync());
        }
    }
}
