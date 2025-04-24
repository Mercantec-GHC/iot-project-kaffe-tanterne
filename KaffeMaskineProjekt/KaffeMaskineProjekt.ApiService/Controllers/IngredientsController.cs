using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProject.DomainModels;
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
