using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Jogos_Academicos.Data;
using Jogos_Academicos.Models.ViewModels;

namespace Jogos_Academicos.Controllers
{
    public class AcessoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcessoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Se já estiver logado, joga para o dashboard do Coordenador (exemplo)
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Evento");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Busca usuário no banco (Senha em texto plano por enquanto, como no Java original)
            // Idealmente, depois implementaremos hash (BCrypt)
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email == model.Email && u.Senha == model.Senha);

            if (usuario == null)
            {
                ViewBag.Erro = "Usuário ou senha inválidos!";
                return View(model);
            }

            // Criar as Claims (Dados do Crachá)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NomeCompleto),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario.ToString()) // Importante para autorização!
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Criar o Cookie
            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Redirecionamento baseado no Papel (Role)
            if (usuario.TipoUsuario.ToString() == "Coordenador")
                return RedirectToAction("Index", "Evento");

            // Default
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
    }
}