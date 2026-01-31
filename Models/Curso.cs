using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Jogos_Academicos.Models.Enums;

namespace Jogos_Academicos.Models
{
    [Table("cursos")]
    public class Curso
    {
        [Key]
        [Column("id_cursos")]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Column("nivel")]
        public Grau Nivel { get; set; }

        [Column("fk_campus")]
        public int CampusId { get; set; }
        public virtual Campus Campus { get; set; }

        public virtual ICollection<Equipe> Equipes { get; set; }


        // Remover ao criar a classe atleta
        public virtual ICollection<Usuario> Atletas { get; set; }
    }
}