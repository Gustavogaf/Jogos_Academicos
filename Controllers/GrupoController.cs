using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jogos_Academicos.Data;
using Jogos_Academicos.Services;

namespace Jogos_Academicos.Controllers
{
    [Authorize]
    public class GrupoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeradorPartidasService _geradorService;

        public GrupoController(ApplicationDbContext context, GeradorPartidasService geradorService)
        {
            _context = context;
            _geradorService = geradorService;
        }

        // GET: Grupo/Detalhes/5
        public async Task<IActionResult> Detalhes(int id)
        {
            // 1. Busca dados do Grupo e Jogos
            var grupo = await _context.Grupos
                .Include(g => g.Equipes)
                .Include(g => g.Jogos).ThenInclude(j => j.EquipeA)
                .Include(g => g.Jogos).ThenInclude(j => j.EquipeB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (grupo == null) return NotFound();

            // 2. Busca a Classificação Ordenada (Sem mexer no Model Grupo para evitar erro de FK)
            var classificacao = await _context.Classificacoes
                .Where(c => c.GrupoId == id)
                .Include(c => c.Equipe)
                .OrderByDescending(c => c.Pontos)
                .ThenByDescending(c => c.Vitorias)
                .ThenByDescending(c => c.SaldoGols)
                .ToListAsync();

            // Passamos a lista separada via ViewBag para facilitar
            ViewBag.Classificacao = classificacao;

            return View(grupo);
        }

        // POST: Grupo/GerarJogos
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordenador")] // Só coordenador gera tabela
        public async Task<IActionResult> GerarJogos(int grupoId)
        {
            try
            {
                await _geradorService.GerarJogosGrupo(grupoId);
                TempData["Sucesso"] = "Tabela de jogos gerada com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalhes), new { id = grupoId });
        }
    }
}