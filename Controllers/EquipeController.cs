using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.enums;
using Jogos_Academicos.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jogos_Academicos.Controllers
{
    [Authorize(Roles = "Tecnico")]
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Forbid();

            int userId = int.Parse(userIdStr);

            var equipes = await _context.Equipes
                .Include(e => e.Esporte)
                .Include(e => e.Curso)
                .Include(e => e.Atletas) // <--- ADICIONE ESTA LINHA
                .Where(e => e.TecnicoId == userId)
                .ToListAsync();

            return View(equipes);
        }

        // GET: Equipe/Criar
        public async Task<IActionResult> Criar()
        {
            var tecnicoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 1. Busca os dados do Técnico para saber o Curso dele
            var tecnico = await _context.Usuarios
                .Include(u => u.Curso)
                .FirstOrDefaultAsync(u => u.Id == tecnicoId);

            if (tecnico?.CursoId == null)
            {
                // Regra de segurança: Técnico sem curso não cria equipe
                TempData["Erro"] = "Você precisa estar vinculado a um curso para criar equipes.";
                return RedirectToAction("Index", "Home");
            }

            // 2. Carrega apenas Atletas do MESMO curso do Técnico
            var atletasDisponiveis = await _context.Usuarios
                .Where(u => u.CursoId == tecnico.CursoId && u.TipoUsuario == Role.Atleta)
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            // 3. Prepara a View
            // Passamos o nome do curso para exibir (campo readonly)
            ViewBag.NomeCurso = tecnico.Curso.Nome;

            // MultiSelectList permite selecionar vários atletas
            ViewBag.Atletas = new MultiSelectList(atletasDisponiveis, "Id", "NomeCompleto");

            // Dropdown de esportes continua normal
            ViewBag.EsporteId = new SelectList(_context.Esportes, "Id", "Nome");

            return View();
        }

        // POST: Equipe/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Recebemos 'atletasIds' do <select multiple>
        public async Task<IActionResult> Criar(Equipe equipe, List<int> atletasIds)
        {
            var tecnicoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Busca o técnico novamente para garantir a segurança do CursoId
            var tecnico = await _context.Usuarios.FindAsync(tecnicoId);

            // REGRA: Curso da equipe = Curso do Técnico (Ignora o que vier da View se houver manipulação)
            equipe.CursoId = tecnico.CursoId;
            equipe.TecnicoId = tecnicoId;

            // REGRA: Adicionar os Atletas selecionados
            if (atletasIds != null && atletasIds.Any())
            {
                // Busca os usuários no banco
                var atletasParaAdicionar = await _context.Usuarios
                    .Where(u => atletasIds.Contains(u.Id))
                    .ToListAsync();

                equipe.Atletas = atletasParaAdicionar;
            }

            // Remove validações que não vêm do formulário
            ModelState.Remove("Tecnico");
            ModelState.Remove("Esporte");
            ModelState.Remove("Curso");
            ModelState.Remove("Grupos");
            ModelState.Remove("Atletas");

            if (ModelState.IsValid)
            {
                _context.Add(equipe);
                await _context.SaveChangesAsync(); // O EF Core salva a tabela 'equipe_atleta' automaticamente aqui

                TempData["Sucesso"] = "Equipe criada com sucesso!";
                return RedirectToAction(nameof(MinhasEquipes));
            }

            // Se der erro (ex: nome vazio), recarrega as listas para não quebrar a tela
            var atletasDisponiveis = await _context.Usuarios
                .Where(u => u.CursoId == tecnico.CursoId && u.TipoUsuario == Role.Atleta)
                .ToListAsync();

            ViewBag.NomeCurso = _context.Cursos.Find(tecnico.CursoId)?.Nome;
            ViewBag.Atletas = new MultiSelectList(atletasDisponiveis, "Id", "NomeCompleto", atletasIds);
            ViewBag.EsporteId = new SelectList(_context.Esportes, "Id", "Nome", equipe.EsporteId);

            return View(equipe);
        }
    }
}