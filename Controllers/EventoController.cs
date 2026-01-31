using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Services;

namespace Jogos_Academicos.Controllers
{
    [Authorize]
    public class EventoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeradorPartidasService _geradorService;

        public EventoController(ApplicationDbContext context, GeradorPartidasService geradorService)
        {
            _context = context;
            _geradorService = geradorService;
        }

        // --- ÁREA DO COORDENADOR ---

        [Authorize(Roles = "Coordenador")]
        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Eventos
                .Include(e => e.Esporte)
                .Include(e => e.Grupos)
                .ToListAsync();
            return View(eventos);
        }

        [Authorize(Roles = "Coordenador")]
        public IActionResult Criar()
        {
            ViewBag.Esportes = _context.Esportes.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordenador")]
        public async Task<IActionResult> Criar(Evento evento)
        {
            ModelState.Remove("Esporte");
            ModelState.Remove("Grupos");

            if (ModelState.IsValid)
            {
                _context.Add(evento);
                await _context.SaveChangesAsync();

                // Automação 1: Coordenador cria o evento -> Sistema cria grupos vazios ou preenchidos
                try
                {
                    await _geradorService.DistribuirEquipesEmGrupos(evento.Id);
                    TempData["Sucesso"] = "Evento e grupos criados automaticamente!";
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = $"Evento criado, mas erro nos grupos: {ex.Message}";
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Esportes = _context.Esportes.ToList();
            return View(evento);
        }

        // --- ÁREA DO ÁRBITRO (GESTÃO) ---

        [Authorize(Roles = "Arbitro")]
        public async Task<IActionResult> PainelArbitro()
        {
            var eventos = await _context.Eventos
                .Include(e => e.Esporte)
                .Include(e => e.Grupos).ThenInclude(g => g.Jogos) // Importante para ver status
                .ToListAsync();
            return View(eventos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Arbitro")]
        public async Task<IActionResult> GerarJogosFaseGrupos(int eventoId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Forbid();
            int arbitroId = int.Parse(userIdStr);

            // Chama o serviço passando o ID do Árbitro logado
            var (sucesso, mensagem) = await _geradorService.GerarJogosFaseGrupos(eventoId, arbitroId);

            if (sucesso) TempData["Sucesso"] = mensagem;
            else TempData["Erro"] = mensagem;

            return RedirectToAction(nameof(PainelArbitro));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Arbitro")]
        public async Task<IActionResult> AvancarFase(int eventoId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int arbitroId = int.Parse(userIdStr!);

            var (sucesso, mensagem) = await _geradorService.GerarProximaFase(eventoId, arbitroId);

            if (sucesso) TempData["Sucesso"] = mensagem;
            else TempData["Erro"] = mensagem;

            return RedirectToAction(nameof(PainelArbitro));
        }

        [AllowAnonymous] // Permite acesso público (ajuste se necessário)
        public async Task<IActionResult> VisualizarMataMata(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.Esporte)
                .Include(e => e.Jogos).ThenInclude(j => j.EquipeA)
                .Include(e => e.Jogos).ThenInclude(j => j.EquipeB)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null) return NotFound();

            // Filtra apenas jogos que NÃO são de grupo
            var jogosMataMata = evento.Jogos
                .Where(j => j.Fase != Jogos_Academicos.Models.Enums.FaseTorneio.Grupos)
                .OrderByDescending(j => j.Fase) // Ordena da Final para trás
                .ThenBy(j => j.DataHora)
                .ToList();

            ViewBag.EventoNome = evento.Nome;
            return View(jogosMataMata);
        }
    }
}