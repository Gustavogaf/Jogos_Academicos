using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.Enums; // Necessário para acessar FaseTorneio
using Jogos_Academicos.Models.ViewModels;
using Jogos_Academicos.Services;

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
        [Authorize(Roles = "Arbitro")]
        public async Task<IActionResult> MinhasPartidas()
        {
            // Pega o ID do usuário logado via Cookie
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Forbid();

            int userId = int.Parse(userIdStr);

            var jogos = await _context.Jogos
                .Include(j => j.EquipeA)
                .Include(j => j.EquipeB)
                .Include(j => j.Evento)
                .Where(j => j.ArbitroId == userId && !j.Finalizado) // Só jogos pendentes desse árbitro
                .OrderBy(j => j.DataHora)
                .ToListAsync();

            return View(jogos);
        }

        // POST: Jogo/RegistrarResultado
        [HttpPost]
        [Authorize(Roles = "Arbitro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarResultado(ResultadoViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Forbid();

            int userId = int.Parse(userIdStr);

            var jogo = await _context.Jogos
                .Include(j => j.Grupo)
                .FirstOrDefaultAsync(j => j.Id == model.JogoId);

            if (jogo == null) return NotFound();

            // 1. Validação de Segurança: O jogo é mesmo desse árbitro?
            if (jogo.ArbitroId != userId)
                return Forbid();

            // 2. NOVA VALIDAÇÃO: Impedir empate em fase eliminatória (Mata-Mata)
            bool isMataMata = jogo.Fase != FaseTorneio.Grupos;
            bool isEmpate = (model.PlacarA == model.PlacarB) && !model.WoA && !model.WoB;

            if (isMataMata && isEmpate)
            {
                // Como estamos redirecionando para uma lista, usamos TempData para mostrar o erro
                TempData["Erro"] = "Jogos de fase eliminatória não podem terminar empatados. Realize o desempate.";
                return RedirectToAction(nameof(MinhasPartidas));
            }

            // 3. Atualiza o Placar e Finaliza
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
                TempData["Erro"] = "Informe o placar ou marque W.O.";
                return RedirectToAction(nameof(MinhasPartidas));
            }

            // Salva as alterações do jogo
            await _context.SaveChangesAsync();

            // 4. Se for Fase de Grupos, atualiza a tabela de classificação
            if (jogo.Fase == FaseTorneio.Grupos && jogo.GrupoId.HasValue)
            {
                await _classificacaoService.AtualizarClassificacao(jogo.GrupoId.Value);
            }

            TempData["Sucesso"] = "Resultado registrado com sucesso!";
            return RedirectToAction(nameof(MinhasPartidas));
        }
    }
}