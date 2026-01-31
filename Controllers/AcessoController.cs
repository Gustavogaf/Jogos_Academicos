using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.enums;
using Jogos_Academicos.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization; // Importante para [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jogos_Academicos.Controllers
{
    public class AcessoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcessoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- CADASTRO PÚBLICO (Apenas para Atletas) ---
        [HttpGet]
        public IActionResult Cadastro()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

            // Carrega a lista de cursos para o Dropdown
            ViewBag.Cursos = _context.Cursos.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastro(Usuario usuario)
        {
            // 1. Validação de Email Duplicado
            if (_context.Usuarios.Any(u => u.Email == usuario.Email))
            {
                ViewBag.Erro = "Este email já está cadastrado.";
                ViewBag.Cursos = _context.Cursos.ToList(); // Recarrega cursos em caso de erro
                return View(usuario);
            }

            // 2. Validação de Matrícula Duplicada
            if (_context.Usuarios.Any(u => u.Matricula == usuario.Matricula))
            {
                ViewBag.Erro = "Esta matrícula já possui cadastro.";
                ViewBag.Cursos = _context.Cursos.ToList();
                return View(usuario);
            }

            // 3. Força os dados padrão de Atleta
            usuario.TipoUsuario = Role.Atleta;
            usuario.DataCriacao = DateTime.Now;

            // 4. Limpa validações de propriedades de navegação (que não vêm do formulário)
            ModelState.Remove("Curso");
            ModelState.Remove("Equipes");
            ModelState.Remove("Jogos"); // Caso exista no model

            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Redireciona para o Login com uma mensagem (opcional usar TempData)
                return RedirectToAction("Login");
            }

            // Se algo falhou, recarrega a lista e devolve a tela
            ViewBag.Cursos = _context.Cursos.ToList();
            return View(usuario);
        }

        // --- CADASTRO RESTRITO (Apenas Coordenador cria Staff) ---
        /*
        [Authorize(Roles = "Coordenador")]
        [HttpGet]
        public IActionResult RegistrarUsuario()
        {
            return View();
        }

        [Authorize(Roles = "Coordenador")]
        [HttpPost]
        public async Task<IActionResult> RegistrarUsuario(Usuario usuario)
        {
            // Validação de email
            if (_context.Usuarios.Any(u => u.Email == usuario.Email))
            {
                ViewBag.Erro = "Este email já está cadastrado.";
                return View(usuario);
            }

            // REGRA: Coordenador não pode criar outro Coordenador (opcional, mas seguro)
            // E também não deve criar Atletas por aqui (atletas usam o cadastro público)
            if (usuario.TipoUsuario == Role.Coordenador || usuario.TipoUsuario == Role.Atleta)
            {
                ViewBag.Erro = "Esta tela é exclusiva para cadastro de Técnicos e Árbitros.";
                return View(usuario);
            }

            usuario.DataCriacao = DateTime.Now;

            // Removemos validações de navegação desnecessárias para criação simples
            ModelState.Remove("Curso");
            ModelState.Remove("Equipes");

            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                ViewBag.Sucesso = "Usuário cadastrado com sucesso!";
                return View(); // Mantém na tela para cadastrar outro se quiser
            }

            return View(usuario);
        } */
        [HttpGet]
        [Authorize(Roles = "Coordenador")]
        public async Task<IActionResult> GerenciarAtletas()
        {
            // Busca apenas usuários que são ATLETAS
            var atletas = await _context.Usuarios
                .Where(u => u.TipoUsuario == Role.Atleta)
                .Include(u => u.Curso) // Inclui curso para visualização
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            return View(atletas);
        }

        // POST: Transforma um Atleta em Técnico
        [HttpPost]
        [Authorize(Roles = "Coordenador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoverParaTecnico(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction(nameof(GerenciarAtletas));
            }

            if (usuario.TipoUsuario != Role.Atleta)
            {
                TempData["Erro"] = "Apenas atletas podem ser promovidos a técnico.";
                return RedirectToAction(nameof(GerenciarAtletas));
            }

            // A MÁGICA: Muda a Role para Técnico
            usuario.TipoUsuario = Role.Tecnico;

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"O usuário {usuario.NomeCompleto} agora é um Técnico!";
            return RedirectToAction(nameof(GerenciarAtletas));
        }

        // --- LOGIN E LOGOUT (Mantidos iguais) ---
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email == model.Email && u.Senha == model.Senha);

            if (usuario == null)
            {
                ViewBag.Erro = "Usuário ou senha inválidos!";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NomeCompleto),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Redirecionamento Inteligente
            if (usuario.TipoUsuario == Role.Coordenador) return RedirectToAction("Index", "Evento");
            if (usuario.TipoUsuario == Role.Arbitro) return RedirectToAction("MinhasPartidas", "Jogo");
            if (usuario.TipoUsuario == Role.Tecnico) return RedirectToAction("MinhasEquipes", "Equipe"); // Vamos criar esse depois

            return RedirectToAction("Index", "Home"); // Atleta vai pra Home
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
    }
}