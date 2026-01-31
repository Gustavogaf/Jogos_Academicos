using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.enums;
using Microsoft.EntityFrameworkCore;

namespace Jogos_Academicos.Services
{
    public class GeradorPartidasService
    {
        private readonly ApplicationDbContext _context;

        public GeradorPartidasService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GerarJogosGrupo(int grupoId)
        {
            // 1. Busca o grupo e suas equipes
            var grupo = await _context.Grupos
                .Include(g => g.Equipes)
                .Include(g => g.Evento)
                .FirstOrDefaultAsync(g => g.Id == grupoId);

            if (grupo == null || !grupo.Equipes.Any()) return;

            // 2. Verifica se já existem jogos (para não duplicar)
            bool jaTemJogos = await _context.Jogos.AnyAsync(j => j.GrupoId == grupoId);
            if (jaTemJogos) throw new Exception("Este grupo já possui jogos gerados.");

            // 3. Busca lista de árbitros disponíveis (Usuarios com role ARBITRO)
            var arbitros = await _context.Usuarios
                .Where(u => u.TipoUsuario == Models.enums.Role.Arbitro)
                .ToListAsync();

            if (!arbitros.Any()) throw new Exception("Não há árbitros cadastrados no sistema.");

            // 4. Algoritmo Round Robin (Todos contra Todos)
            var equipes = grupo.Equipes.ToList();
            int numEquipes = equipes.Count;

            // Se for ímpar, adiciona um time "fantasma" (null) para indicar folga
            if (numEquipes % 2 != 0)
            {
                equipes.Add(null);
                numEquipes++;
            }

            int numRodadas = numEquipes - 1;
            int jogosPorRodada = numEquipes / 2;
            Random random = new Random();

            for (int rodada = 0; rodada < numRodadas; rodada++)
            {
                for (int jogoIdx = 0; jogoIdx < jogosPorRodada; jogoIdx++)
                {
                    var timeA = equipes[jogoIdx];
                    var timeB = equipes[numEquipes - 1 - jogoIdx];

                    // Se um dos times for null (fantasma), é folga, não cria jogo
                    if (timeA != null && timeB != null)
                    {
                        var jogo = new Jogo
                        {
                            GrupoId = grupoId,
                            EventoId = grupo.EventoId,
                            EquipeAId = timeA.Id,
                            EquipeBId = timeB.Id,
                            DataHora = DateTime.Now.AddDays(rodada + 1), // Agenda 1 rodada por dia (simulação)
                            Finalizado = false,

                            // Sorteia um árbitro aleatório
                            ArbitroId = arbitros[random.Next(arbitros.Count)].Id
                        };

                        _context.Jogos.Add(jogo);
                    }
                }

                // Rotacionar a lista (mantém o primeiro fixo e gira os outros)
                // Ex: [0, 1, 2, 3] -> fixa 0 -> [0, 3, 1, 2]
                var ultimo = equipes[numEquipes - 1];
                equipes.RemoveAt(numEquipes - 1);
                equipes.Insert(1, ultimo);
            }

            await _context.SaveChangesAsync();
        }
    }
}