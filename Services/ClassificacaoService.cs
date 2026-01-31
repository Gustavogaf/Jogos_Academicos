using Microsoft.EntityFrameworkCore;
using Jogos_Academicos.Data;
using Jogos_Academicos.Models;

namespace Jogos_Academicos.Services
{
    public class ClassificacaoService
    {
        private readonly ApplicationDbContext _context;

        public ClassificacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AtualizarClassificacao(int grupoId)
        {
            // 1. Buscar o grupo e suas equipes
            var grupo = await _context.Grupos
                .Include(g => g.Equipes)
                .FirstOrDefaultAsync(g => g.Id == grupoId);

            if (grupo == null) return;

            // 2. Buscar ou Criar as entradas de classificação para cada time
            var classificacoes = await _context.Classificacoes
                .Where(c => c.GrupoId == grupoId)
                .ToListAsync();

            // Garante que todo time do grupo tenha uma linha na tabela
            foreach (var equipe in grupo.Equipes)
            {
                if (!classificacoes.Any(c => c.EquipeId == equipe.Id))
                {
                    var nova = new ClassificacaoGrupo { GrupoId = grupoId, EquipeId = equipe.Id };
                    _context.Classificacoes.Add(nova);
                    classificacoes.Add(nova);
                }
            }

            // 3. Resetar estatísticas (Zerar tudo para recalcular)
            foreach (var c in classificacoes)
            {
                c.Pontos = 0; c.Vitorias = 0; c.Empates = 0; c.Derrotas = 0; c.SaldoGols = 0;
            }

            // 4. Buscar jogos FINALIZADOS do grupo
            var jogos = await _context.Jogos
                .Where(j => j.GrupoId == grupoId && j.Finalizado)
                .ToListAsync();

            // 5. Processar cada jogo
            foreach (var jogo in jogos)
            {
                var timeA = classificacoes.FirstOrDefault(c => c.EquipeId == jogo.EquipeAId);
                var timeB = classificacoes.FirstOrDefault(c => c.EquipeId == jogo.EquipeBId);

                if (timeA == null || timeB == null) continue;

                // Lógica de WO
                if (jogo.WoA) // A perdeu por WO
                {
                    ContabilizarVitoria(timeB, 0); // Geralmente WO é 3x0 ou 1x0, ajuste o saldo se quiser
                    ContabilizarDerrota(timeA, 0);
                }
                else if (jogo.WoB) // B perdeu por WO
                {
                    ContabilizarVitoria(timeA, 0);
                    ContabilizarDerrota(timeB, 0);
                }
                else // Jogo Normal
                {
                    int golsA = jogo.PlacarA ?? 0;
                    int golsB = jogo.PlacarB ?? 0;
                    int saldo = golsA - golsB;

                    if (golsA > golsB)
                    {
                        ContabilizarVitoria(timeA, saldo);
                        ContabilizarDerrota(timeB, -saldo);
                    }
                    else if (golsB > golsA)
                    {
                        ContabilizarVitoria(timeB, -saldo);
                        ContabilizarDerrota(timeA, saldo);
                    }
                    else // Empate
                    {
                        timeA.Empates++;
                        timeA.Pontos += 1;
                        timeB.Empates++;
                        timeB.Pontos += 1;
                        // Saldo não muda no empate
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private void ContabilizarVitoria(ClassificacaoGrupo c, int saldo)
        {
            c.Vitorias++;
            c.Pontos += 3;
            c.SaldoGols += saldo;
        }

        private void ContabilizarDerrota(ClassificacaoGrupo c, int saldo)
        {
            c.Derrotas++;
            c.SaldoGols += saldo;
        }
    }
}
