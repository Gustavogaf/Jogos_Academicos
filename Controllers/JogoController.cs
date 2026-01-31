using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.ViewModels;
using Jogos_Academicos.Services; // Importante

namespace Jogos_Academicos.Controllers
{
    [Authorize]
    public class JogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ClassificacaoService _classificacaoService;

        public JogoController(ApplicationDbContext context, ClassificacaoService classificacaoService)
        {
            _context = context;
            _classificacaoService = classificacaoService;
        }

        // GET: Jogo/MinhasPartidas (Apenas para Árbitros)
        [Authorize(Roles = "Arbitro")] // Garante que só árbitro acessa
        public async Task<IActionResult> MinhasPartidas()
        {
            // Pega o ID do usuário logado via Cookie
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var jogos = await _context.Jogos // Atenção: O DbSet<Jogo> precisa existir no Context (será criado no passo 3 se não houver)
                .Include(j => j.EquipeA)
                .Include(j => j.EquipeB)
                .Include(j => j.Evento)
                .Where(j => j.ArbitroId == userId && !j.Finalizado) // Só jogos pendentes desse árbitro
                .ToListAsync();

            return View(jogos);
        }

        // POST: Jogo/RegistrarResultado
        [HttpPost]
        [Authorize(Roles = "Arbitro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarResultado(ResultadoViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var jogo = await _context.Jogos
                .Include(j => j.Grupo)
                .FirstOrDefaultAsync(j => j.Id == model.JogoId);

            if (jogo == null) return NotFound();

            // Validação de Segurança: O jogo é mesmo desse árbitro?
            if (jogo.ArbitroId != userId)
                return Forbid();

            // Atualiza o Placar
            if (model.WoA)
            {
                jogo.WoA = true;
                jogo.Finalizado = true;
            }
            else if (model.WoB)
            {
                jogo.WoB = true;
                jogo.Finalizado = true;
            }
            else if (model.PlacarA.HasValue && model.PlacarB.HasValue)
            {
                jogo.PlacarA = model.PlacarA;
                jogo.PlacarB = model.PlacarB;
                jogo.Finalizado = true;
            }
            else
            {
                ModelState.AddModelError("", "Informe o placar ou marque W.O.");
                return RedirectToAction(nameof(MinhasPartidas));
            }

            

            await _context.SaveChangesAsync();
            await _classificacaoService.AtualizarClassificacao(jogo.GrupoId);
            return RedirectToAction(nameof(MinhasPartidas));
        }
    }
}
