using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jogos_Academicos.Models
{
    [Table("campus")]
    public class Campus
    {
        [Key]
        [Column("id_campus")]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Endereco { get; set; }

        // Relacionamento: Um Campus tem vários Cursos
        public virtual ICollection<Curso> Cursos { get; set; } = new List<Curso>();
    }
}