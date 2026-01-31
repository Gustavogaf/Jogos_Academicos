using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Necessário para pegar o ID do usuário

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
        // GET: Grupo/Criar?eventoId=5
        [Authorize(Roles = "Coordenador")]
        public IActionResult Criar(int eventoId)
        {
            ViewBag.EventoId = eventoId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordenador")]
        public async Task<IActionResult> Criar(Grupo grupo)
        {
            // Validação básica
            if (string.IsNullOrWhiteSpace(grupo.Nome))
                ModelState.AddModelError("Nome", "O nome do grupo é obrigatório");

            if (ModelState.IsValid)
            {
                _context.Add(grupo);
                await _context.SaveChangesAsync();
                // Redireciona para adicionar equipes logo após criar
                return RedirectToAction(nameof(GerenciarEquipes), new { grupoId = grupo.Id });
            }

            ViewBag.EventoId = grupo.EventoId;
            return View(grupo);
        }

        // --- PARTE 2: ESCOLHER AS EQUIPES DO GRUPO ---

        // GET: Grupo/GerenciarEquipes/10
        [Authorize(Roles = "Coordenador")]
        public async Task<IActionResult> GerenciarEquipes(int grupoId)
        {
            var grupo = await _context.Grupos
                .Include(g => g.Evento)
                .Include(g => g.Equipes)
                .FirstOrDefaultAsync(g => g.Id == grupoId);

            if (grupo == null) return NotFound();

            // Lógica de Seleção:
            // 1. Equipes do mesmo Esporte do Evento.
            // 2. Que NÃO estejam em outro grupo DESTE evento (para não duplicar).
            // 3. OU que já estejam neste grupo (para manter a seleção atual).

            var equipesDisponiveis = await _context.Equipes
                .Where(e => e.EsporteId == grupo.Evento.EsporteId)
                .Where(e => !e.Grupos.Any(g => g.EventoId == grupo.EventoId && g.Id != grupoId))
                .ToListAsync();

            ViewBag.Equipes = new MultiSelectList(equipesDisponiveis, "Id", "Nome", grupo.Equipes.Select(e => e.Id));
            return View(grupo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordenador")]
        public async Task<IActionResult> GerenciarEquipes(int grupoId, List<int> equipesIds)
        {
            var grupo = await _context.Grupos
                .Include(g => g.Equipes)
                .FirstOrDefaultAsync(g => g.Id == grupoId);

            if (grupo == null) return NotFound();

            // Limpa as equipes atuais e adiciona as novas selecionadas
            grupo.Equipes.Clear();

            if (equipesIds != null)
            {
                var equipesSelecionadas = await _context.Equipes
                    .Where(e => equipesIds.Contains(e.Id))
                    .ToListAsync();

                foreach (var equipe in equipesSelecionadas)
                {
                    grupo.Equipes.Add(equipe);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Equipes do grupo atualizadas com sucesso!";

            return RedirectToAction("Detalhes", "Evento", new { id = grupo.EventoId }); // Volta pro Evento
        }

        // GET: Grupo/Detalhes/5
        public async Task<IActionResult> Detalhes(int id)
        {
            // (Mantém o código de Detalhes exatamente como estava)
            var grupo = await _context.Grupos
                .Include(g => g.Equipes)
                .Include(g => g.Jogos).ThenInclude(j => j.EquipeA)
                .Include(g => g.Jogos).ThenInclude(j => j.EquipeB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (grupo == null) return NotFound();

            var classificacao = await _context.Classificacoes
                .Where(c => c.GrupoId == id)
                .Include(c => c.Equipe)
                .OrderByDescending(c => c.Pontos)
                .ThenByDescending(c => c.Vitorias)
                .ThenByDescending(c => c.SaldoGols)
                .ToListAsync();

            ViewBag.Classificacao = classificacao;
            return View(grupo);
        }

    }
}