using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.enums; // Ajuste o namespace se necessário

namespace Jogos_Academicos.Controllers
{
    [Authorize(Roles = "Tecnico")] // Apenas Técnicos acessam esta área
    public class EquipeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EquipeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Equipe/MinhasEquipes
        public async Task<IActionResult> MinhasEquipes()
        {
            // Obtém o ID do usuário logado
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Forbid();

            int userId = int.Parse(userIdStr);

            // Busca as equipes onde o Técnico é o usuário logado
            var equipes = await _context.Equipes
                .Include(e => e.Esporte)
                .Include(e => e.Curso)
                .Where(e => e.TecnicoId == userId)
                .ToListAsync();

            return View(equipes);
        }

        // GET: Equipe/Criar
        public IActionResult Criar()
        {
            // Carrega os dropdowns de Esportes e Cursos
            ViewData["EsporteId"] = new SelectList(_context.Esportes, "Id", "Nome");
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nome");
            return View();
        }

        // POST: Equipe/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Equipe equipe)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Forbid();

            // Vincula a equipe ao Técnico logado
            equipe.TecnicoId = int.Parse(userIdStr);

            // Remove validações de propriedades de navegação
            ModelState.Remove("Tecnico");
            ModelState.Remove("Esporte");
            ModelState.Remove("Curso");
            ModelState.Remove("Atletas");
            ModelState.Remove("Grupos");

            if (ModelState.IsValid)
            {
                _context.Add(equipe);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Equipe cadastrada com sucesso!";
                return RedirectToAction(nameof(MinhasEquipes));
            }

            // Se falhar, recarrega os dropdowns
            ViewData["EsporteId"] = new SelectList(_context.Esportes, "Id", "Nome", equipe.EsporteId);
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "Nome", equipe.CursoId);
            return View(equipe);
        }
    }
}