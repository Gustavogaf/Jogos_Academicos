using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Jogos_Academicos.Models
{
    [Table("equipes")]
    public class Equipe
    {
        [Key]
        [Column("id_equipe")]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        // Técnico (Usuario)
        [Column("fk_tecnico")]
        public int? TecnicoId { get; set; }
        [ForeignKey("TecnicoId")]
        public virtual Usuario Tecnico { get; set; }

        // Esporte
        [Column("fk_esporte")]
        public int EsporteId { get; set; }
        public virtual Esporte Esporte { get; set; }

        // Curso
        [Column("fk_curso")]
        public int? CursoId { get; set; }
        public virtual Curso Curso { get; set; }

        // Many-to-Many: Atletas (Usuarios)
        // O EF Core cria a tabela 'equipe_atleta' automaticamente se configurado corretamente
        public virtual ICollection<Usuario> Atletas { get; set; } = new List<Usuario>();

        // Many-to-Many: Grupos
        public virtual ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();

        // Jogos (serão adicionados na Task do Dev 03)
    }
}