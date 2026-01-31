using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Importante
using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using System.Diagnostics;

namespace Jogos_Academicos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Se não estiver logado, manda pro login
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Acesso");
            }

            // Busca eventos ativos para mostrar no dashboard
            var eventos = await _context.Eventos
                .Include(e => e.Esporte)
                .Include(e => e.Grupos)
                .OrderByDescending(e => e.Data)
                .ToListAsync();

            return View(eventos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}