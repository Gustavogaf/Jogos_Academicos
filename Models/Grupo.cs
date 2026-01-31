using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jogos_Academicos.Models
{
    [Table("grupos")]
    public class Grupo
    {
        [Key]
        [Column("id_grupo")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do grupo é obrigatório")]
        public string Nome { get; set; }

        [Column("fk_eventos")]
        public int EventoId { get; set; }
        public virtual Evento Evento { get; set; }

        // Many-to-Many com Equipes
        public virtual ICollection<Equipe> Equipes { get; set; } = new List<Equipe>();

        public virtual ICollection<Jogo> Jogos { get; set; } = new List<Jogo>();
    }
}