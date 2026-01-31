using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.Enums;

namespace Jogos_Academicos.Controllers
{
    [Authorize] // Exige login para acessar qualquer coisa aqui
    public class EventoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Evento/Index (Dashboard do Coordenador)
        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Eventos
                .Include(e => e.Esporte) // Join com Esporte para mostrar o nome
                .Include(e => e.Grupos)
                .ToListAsync();

            return View(eventos);
        }

        // GET: Evento/Criar
        public IActionResult Criar()
        {
            // Carrega a lista de Esportes para o Dropdown (Select)
            ViewBag.Esportes = _context.Esportes.ToList();
            return View();
        }

        // POST: Evento/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Evento evento)
        {
            // Remove validações de navegação que não vêm do form
            ModelState.Remove("Esporte");
            ModelState.Remove("Grupos");

            if (ModelState.IsValid)
            {
                _context.Add(evento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Se der erro, recarrega a lista de esportes
            ViewBag.Esportes = _context.Esportes.ToList();
            return View(evento);
        }
    }
}
