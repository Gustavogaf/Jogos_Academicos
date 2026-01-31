using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jogos_Academicos.Models
{
    [Table("esportes")]
    public class Esporte
    {
        [Key]
        [Column("id_esporte")]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public virtual ICollection<Equipe> Equipes { get; set; }
        public virtual ICollection<Evento> Eventos { get; set; }
    }
}