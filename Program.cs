using Jogos_Academicos.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DOS SERVIÇOS (DI) ---

// Configuração do Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configuração da Autenticação (Cookies)
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "SistemaAcademico.Cookie";
        options.LoginPath = "/Acesso/Login"; // Redireciona para cá se não estiver logado
        options.AccessDeniedPath = "/Acesso/Negado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Adiciona suporte a Controllers e Views (MVC)
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Jogos_Academicos.Services.ClassificacaoService>();
builder.Services.AddScoped<Jogos_Academicos.Services.GeradorPartidasService>();
var app = builder.Build();

// --- 2. CONFIGURAÇÃO DO PIPELINE (MIDDLEWARE) ---

// Tratamento de erros em produção
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Importante para carregar CSS/JS do wwwroot

app.UseRouting();

// --- ORDEM CRÍTICA: Autenticação ANTES de Autorização ---
app.UseAuthentication(); // Verifica "Quem é você?" (Lê o Cookie)
app.UseAuthorization();  // Verifica "O que você pode fazer?" (Checa as Roles)

// Configuração das Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acesso}/{action=Login}/{id?}"); // Define Login como página inicial

app.Run();