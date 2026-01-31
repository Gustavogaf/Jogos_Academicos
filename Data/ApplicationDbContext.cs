using Microsoft.EntityFrameworkCore;
using Jogos_Academicos.Models;
using Jogos_Academicos.Models.Enums;

namespace Jogos_Academicos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Mapeamento das tabelas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Campus> Campus { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Esporte> Esportes { get; set; }
        public DbSet<Equipe> Equipes { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Grupo> Grupos { get; set; }

        public DbSet<Jogo> Jogos { get; set; }

        public DbSet<ClassificacaoGrupo> Classificacoes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Matricula).IsUnique();
                entity.Property(u => u.TipoUsuario).HasConversion<string>();

                // Evitar cascata: Se deletar Curso, não deletar o Usuario (apenas seta null)
                entity.HasOne(u => u.Curso)
                      .WithMany(c => c.Atletas) // Assumindo que Curso tem lista de Atletas
                      .HasForeignKey(u => u.CursoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Configuração de Relacionamentos Many-to-Many ---

            // Equipe <-> Atleta
            modelBuilder.Entity<Equipe>()
                .HasMany(e => e.Atletas)
                .WithMany()
                .UsingEntity(j => j.ToTable("equipe_atleta"));

            // Grupo <-> Equipe (Onde deu o erro)
            modelBuilder.Entity<Grupo>()
                .HasMany(g => g.Equipes)
                .WithMany(e => e.Grupos)
                .UsingEntity(j => j.ToTable("grupos_equipes"));

            // --- CORREÇÃO DO ERRO DE CICLO (Restrict) ---

            // 1. Esporte -> Equipe (Se deletar Esporte, NÃO deleta Equipe automaticamente)
            modelBuilder.Entity<Equipe>()
                .HasOne(e => e.Esporte)
                .WithMany(s => s.Equipes)
                .HasForeignKey(e => e.EsporteId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Esporte -> Evento (Se deletar Esporte, NÃO deleta Evento automaticamente)
            modelBuilder.Entity<Evento>()
                .HasOne(e => e.Esporte)
                .WithMany(s => s.Eventos)
                .HasForeignKey(e => e.EsporteId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Curso -> Equipe (Prevenção extra)
            modelBuilder.Entity<Equipe>()
                .HasOne(e => e.Curso)
                .WithMany(c => c.Equipes)
                .HasForeignKey(e => e.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Conversão de Enums ---
            modelBuilder.Entity<Curso>().Property(c => c.Nivel).HasConversion<string>();
            modelBuilder.Entity<Evento>().Property(e => e.Nivel).HasConversion<string>();

            modelBuilder.Entity<Jogo>()
                .HasOne(j => j.EquipeA).WithMany().HasForeignKey(j => j.EquipeAId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Jogo>()
                .HasOne(j => j.EquipeB).WithMany().HasForeignKey(j => j.EquipeBId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Jogo>()
                .HasOne(j => j.Arbitro).WithMany().HasForeignKey(j => j.ArbitroId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Jogo>(entity =>
            {
                // Jogo -> Equipe A (Restrict)
                entity.HasOne(j => j.EquipeA)
                      .WithMany()
                      .HasForeignKey(j => j.EquipeAId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Jogo -> Equipe B (Restrict)
                entity.HasOne(j => j.EquipeB)
                      .WithMany()
                      .HasForeignKey(j => j.EquipeBId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Jogo -> Arbitro (Restrict)
                entity.HasOne(j => j.Arbitro)
                      .WithMany()
                      .HasForeignKey(j => j.ArbitroId)
                      .OnDelete(DeleteBehavior.Restrict);

                // --- CORREÇÃO DO ERRO ATUAL (FK_jogos_grupos_GrupoId) ---

                // Jogo -> Grupo (Se deletar Grupo, NÃO deleta Jogo em cascata)
                entity.HasOne(j => j.Grupo)
                      .WithMany(g => g.Jogos) // ou .WithMany(g => g.Jogos) se tiver a lista no Model
                      .HasForeignKey(j => j.GrupoId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Jogo -> Evento (Preventivo: Se deletar Evento, NÃO deleta Jogo em cascata via FK direta)
                entity.HasOne(j => j.Evento)
                      .WithMany()
                      .HasForeignKey(j => j.EventoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração ClassificacaoGrupo
            modelBuilder.Entity<ClassificacaoGrupo>()
                .HasIndex(c => new { c.GrupoId, c.EquipeId }).IsUnique(); // Impede duplicata

            modelBuilder.Entity<ClassificacaoGrupo>()
                .HasOne(c => c.Grupo)
                .WithMany() // Se Grupo tiver lista de Classificacao, ponha .WithMany(g => g.Classificacoes)
                .HasForeignKey(c => c.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassificacaoGrupo>()
                .HasOne(c => c.Equipe)
                .WithMany()
                .HasForeignKey(c => c.EquipeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}