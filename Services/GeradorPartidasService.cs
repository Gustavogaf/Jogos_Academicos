using Jogos_Academicos.Data;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.Enums;
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

        // 1. AUTOMATIZAÇÃO: Distribui equipes em grupos (Chamado ao criar Evento)
        public async Task DistribuirEquipesEmGrupos(int eventoId)
        {
            var evento = await _context.Eventos.FindAsync(eventoId);
            if (evento == null) return;

            // Busca equipes compatíveis (Mesmo Esporte + Mesmo Nível)
            var equipesElegiveis = await _context.Equipes
                .Include(e => e.Curso)
                .Where(e => e.EsporteId == evento.EsporteId && e.Curso.Nivel == evento.Nivel)
                .ToListAsync();

            if (!equipesElegiveis.Any()) return;

            // Embaralha
            var random = new Random();
            equipesElegiveis = equipesElegiveis.OrderBy(x => random.Next()).ToList();

            // Cria grupos de 4 (ou o que for possível)
            int tamanhoGrupo = 4;
            int numGrupos = (int)Math.Ceiling((double)equipesElegiveis.Count / tamanhoGrupo);

            for (int i = 0; i < numGrupos; i++)
            {
                var grupo = new Grupo
                {
                    Nome = $"Grupo {(char)('A' + i)}",
                    EventoId = eventoId,
                    Equipes = equipesElegiveis.Skip(i * tamanhoGrupo).Take(tamanhoGrupo).ToList()
                };
                _context.Grupos.Add(grupo);
            }
            await _context.SaveChangesAsync();
        }

        // 2. AÇÃO DO ÁRBITRO: Gerar tabela de jogos da fase de grupos
        public async Task<(bool Sucesso, string Mensagem)> GerarJogosFaseGrupos(int eventoId, int arbitroId)
        {
            try
            {
                var evento = await _context.Eventos
                    .Include(e => e.Grupos).ThenInclude(g => g.Equipes)
                    .FirstOrDefaultAsync(e => e.Id == eventoId);

                if (evento == null) return (false, "Evento não encontrado.");

                bool jaTemJogos = await _context.Jogos.AnyAsync(j => j.EventoId == eventoId);
                if (jaTemJogos) return (false, "A tabela já foi gerada para este evento.");

                int jogosGerados = 0;

                foreach (var grupo in evento.Grupos)
                {
                    var equipes = grupo.Equipes.ToList();
                    if (equipes.Count < 2) continue;

                    // Ajuste para número ímpar
                    if (equipes.Count % 2 != 0) equipes.Add(null);

                    int numRodadas = equipes.Count - 1;
                    int jogosPorRodada = equipes.Count / 2;

                    for (int r = 0; r < numRodadas; r++)
                    {
                        for (int i = 0; i < jogosPorRodada; i++)
                        {
                            var timeA = equipes[i];
                            var timeB = equipes[equipes.Count - 1 - i];

                            if (timeA != null && timeB != null)
                            {
                                var jogo = new Jogo
                                {
                                    EventoId = eventoId,
                                    GrupoId = grupo.Id,
                                    Fase = FaseTorneio.Grupos,
                                    EquipeAId = timeA.Id,
                                    EquipeBId = timeB.Id,
                                    DataHora = evento.Data.AddDays(r), // 1 rodada por dia
                                    ArbitroId = arbitroId, // O Árbitro assume o jogo aqui
                                    Finalizado = false
                                };
                                _context.Jogos.Add(jogo);
                                jogosGerados++;
                            }
                        }
                        // Rotaciona
                        var ultimo = equipes[equipes.Count - 1];
                        equipes.RemoveAt(equipes.Count - 1);
                        equipes.Insert(1, ultimo);
                    }
                }

                if (jogosGerados == 0) return (false, "Não foi possível gerar jogos (falta de equipes?).");

                await _context.SaveChangesAsync();
                return (true, $"{jogosGerados} jogos gerados com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, $"Erro: {ex.Message}");
            }
        }

        // 3. AÇÃO DO ÁRBITRO: Mata-Mata (Avançar Fase) - IMPLEMENTAÇÃO COMPLETA
        public async Task<(bool Sucesso, string Mensagem)> GerarProximaFase(int eventoId, int arbitroId)
        {
            try
            {
                // --- CORREÇÃO DO BUG DE REGENERAÇÃO ---
                // Problema anterior: OrderByDescending pegava Quartas(8) em vez de Semi(4).
                // Solução: Pegamos a fase com o MENOR valor (exceto Grupos=0), pois 2(Final) < 4(Semi) < 8(Quartas).

                var faseMaisAvancada = await _context.Jogos
                    .Where(j => j.EventoId == eventoId && j.Fase != FaseTorneio.Grupos)
                    .OrderBy(j => j.Fase) // Ascendente: 2 vem antes de 4, que vem antes de 8...
                    .FirstOrDefaultAsync();

                FaseTorneio faseAtual;

                // Se não achou nenhuma fase de mata-mata, assumimos que estamos nos Grupos
                if (faseMaisAvancada == null)
                {
                    faseAtual = FaseTorneio.Grupos;
                }
                else
                {
                    faseAtual = faseMaisAvancada.Fase;
                }

                List<int> idsClassificados = new List<int>();
                FaseTorneio novaFase;

                // CENÁRIO A: SAINDO DA FASE DE GRUPOS
                if (faseAtual == FaseTorneio.Grupos)
                {
                    // Verifica se TODOS os jogos de grupo já acabaram
                    bool jogosPendentes = await _context.Jogos
                        .AnyAsync(j => j.EventoId == eventoId && j.Fase == FaseTorneio.Grupos && !j.Finalizado);

                    if (jogosPendentes) return (false, "Ainda existem jogos de fase de grupos pendentes.");

                    // Busca os 2 melhores de cada grupo
                    var grupos = await _context.Grupos
                        .Where(g => g.EventoId == eventoId)
                        .Include(g => g.Equipes)
                        .ToListAsync();

                    if (!grupos.Any()) return (false, "Nenhum grupo encontrado.");

                    foreach (var grupo in grupos)
                    {
                        var classificadosGrupo = await _context.Classificacoes
                            .Where(c => c.GrupoId == grupo.Id)
                            .OrderByDescending(c => c.Pontos)
                            .ThenByDescending(c => c.Vitorias)
                            .ThenByDescending(c => c.SaldoGols)
                            .Take(2)
                            .Select(c => c.EquipeId)
                            .ToListAsync();

                        if (classificadosGrupo.Count < 2)
                            return (false, $"O Grupo {grupo.Nome} não tem classificação suficiente.");

                        idsClassificados.AddRange(classificadosGrupo);
                    }
                }
                // CENÁRIO B: JÁ ESTAMOS NO MATA-MATA
                else
                {
                    // --- CORREÇÃO DO BYPASS DE VALIDAÇÃO ---
                    // Agora 'faseAtual' é realmente a Semi-Final (4), então ele vai achar os jogos pendentes corretamente.
                    bool fasePendente = await _context.Jogos
                        .AnyAsync(j => j.EventoId == eventoId && j.Fase == faseAtual && !j.Finalizado);

                    if (fasePendente) return (false, $"A fase {faseAtual} ainda tem jogos pendentes. Encerre todas as partidas antes de gerar a próxima.");

                    // Pega os vencedores da fase ATUAL correta
                    var jogosAnteriores = await _context.Jogos
                        .Where(j => j.EventoId == eventoId && j.Fase == faseAtual)
                        .ToListAsync();

                    foreach (var jogo in jogosAnteriores)
                    {
                        // Lógica de quem passou de fase
                        if (jogo.WoA) idsClassificados.Add(jogo.EquipeBId); // Se A deu WO, B vence
                        else if (jogo.WoB) idsClassificados.Add(jogo.EquipeAId); // Se B deu WO, A vence
                        else if (jogo.PlacarA > jogo.PlacarB) idsClassificados.Add(jogo.EquipeAId);
                        else if (jogo.PlacarB > jogo.PlacarA) idsClassificados.Add(jogo.EquipeBId);
                        else
                        {
                            // Tratamento para jogos "Bye" (fictícios)
                            if (jogo.EquipeBId == 0 || jogo.EquipeAId == 0)
                            {
                                idsClassificados.Add(jogo.EquipeAId != 0 ? jogo.EquipeAId : jogo.EquipeBId);
                            }
                        }
                    }
                }

                // 2. Calcular a Nova Fase
                int qtdTimes = idsClassificados.Count;
                if (qtdTimes < 2) return (true, "Torneio Finalizado! Temos um campeão.");

                int alvoChave = 2;
                while (alvoChave < qtdTimes) alvoChave *= 2;

                novaFase = (FaseTorneio)alvoChave;

                // Se a "nova fase" calculada for IGUAL à fase atual, algo deu errado na lógica matemática ou de dados
                if (novaFase == faseAtual) return (false, "Erro: O número de classificados resultaria na mesma fase. Verifique os resultados.");

                // 3. Gerar Confrontos e Byes
                int numeroByes = alvoChave - qtdTimes;
                var timesBye = idsClassificados.Take(numeroByes).ToList();
                var timesJogam = idsClassificados.Skip(numeroByes).ToList();
                int jogosCriados = 0;

                // Jogos Automáticos (Byes)
                foreach (var timeId in timesBye)
                {
                    var jogoBye = new Jogo
                    {
                        EventoId = eventoId,
                        Fase = novaFase,
                        EquipeAId = timeId,
                        EquipeBId = timeId, // Hack técnico para BYE
                        PlacarA = 1,
                        PlacarB = 0,
                        Finalizado = true,
                        DataHora = DateTime.Now,
                        ArbitroId = arbitroId
                    };
                    _context.Jogos.Add(jogoBye);
                }

                // Jogos Reais
                for (int i = 0; i < timesJogam.Count; i += 2)
                {
                    if (i + 1 >= timesJogam.Count) break;

                    var jogo = new Jogo
                    {
                        EventoId = eventoId,
                        GrupoId = null,
                        Fase = novaFase,
                        EquipeAId = timesJogam[i],
                        EquipeBId = timesJogam[i + 1],
                        DataHora = DateTime.Now.AddDays(2),
                        ArbitroId = arbitroId,
                        Finalizado = false
                    };
                    _context.Jogos.Add(jogo);
                    jogosCriados++;
                }

                await _context.SaveChangesAsync();
                return (true, $"Fase {novaFase} gerada com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, $"Erro interno: {ex.Message}");
            }
        }
    }
}