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

            // --- USUÁRIO ---
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Matricula).IsUnique();
                entity.Property(u => u.TipoUsuario).HasConversion<string>();

                entity.HasOne(u => u.Curso)
                      .WithMany(c => c.Atletas)
                      .HasForeignKey(u => u.CursoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- EQUIPE ---
            modelBuilder.Entity<Equipe>(entity =>
            {
                // Equipe <-> Atleta (Many-to-Many)
                entity.HasMany(e => e.Atletas)
                      .WithMany()
                      .UsingEntity(j => j.ToTable("equipe_atleta"));

                // Esporte -> Equipe (Restrict)
                entity.HasOne(e => e.Esporte)
                      .WithMany(s => s.Equipes)
                      .HasForeignKey(e => e.EsporteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Curso -> Equipe (Restrict)
                entity.HasOne(e => e.Curso)
                      .WithMany(c => c.Equipes)
                      .HasForeignKey(e => e.CursoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- GRUPO ---
            modelBuilder.Entity<Grupo>(entity =>
            {
                // Grupo <-> Equipe (Many-to-Many)
                entity.HasMany(g => g.Equipes)
                      .WithMany(e => e.Grupos)
                      .UsingEntity(j => j.ToTable("grupos_equipes"));
            });

            // --- EVENTO ---
            modelBuilder.Entity<Evento>(entity =>
            {
                entity.Property(e => e.Nivel).HasConversion<string>();

                // Esporte -> Evento (Restrict)
                entity.HasOne(e => e.Esporte)
                      .WithMany(s => s.Eventos)
                      .HasForeignKey(e => e.EsporteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- CURSO ---
            modelBuilder.Entity<Curso>()
                .Property(c => c.Nivel).HasConversion<string>();

            // --- JOGO (Configurações consolidadas) ---
            modelBuilder.Entity<Jogo>(entity =>
            {
                // CORREÇÃO CRÍTICA DO EVENTO (Evita EventoId1)
                entity.HasOne(j => j.Evento)
                      .WithMany(e => e.Jogos) // Garante que Evento.Jogos mapeie corretamente
                      .HasForeignKey(j => j.EventoId)
                      .OnDelete(DeleteBehavior.NoAction); // NoAction para evitar ciclos

                // Relacionamento com Grupo (Opcional)
                entity.HasOne(j => j.Grupo)
                      .WithMany(g => g.Jogos)
                      .HasForeignKey(j => j.GrupoId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Equipes e Árbitro (Restrict para evitar cascading delete indesejado)
                entity.HasOne(j => j.EquipeA)
                      .WithMany()
                      .HasForeignKey(j => j.EquipeAId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(j => j.EquipeB)
                      .WithMany()
                      .HasForeignKey(j => j.EquipeBId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(j => j.Arbitro)
                      .WithMany()
                      .HasForeignKey(j => j.ArbitroId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- CLASSIFICAÇÃO ---
            modelBuilder.Entity<ClassificacaoGrupo>(entity =>
            {
                entity.HasIndex(c => new { c.GrupoId, c.EquipeId }).IsUnique();

                entity.HasOne(c => c.Grupo)
                      .WithMany()
                      .HasForeignKey(c => c.GrupoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Equipe)
                      .WithMany()
                      .HasForeignKey(c => c.EquipeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}