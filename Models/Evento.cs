using Jogos_Academicos.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Jogos_Academicos.Models
{
    [Table("eventos")]
    public class Evento
    {
        [Key]
        [Column("id_eventos")]
        public int Id { get; set; }

        public string Nome { get; set; }

        public DateTime Data { get; set; }

        [Column("nivel")]
        public Grau Nivel { get; set; }

        [Column("fk_esporte")]
        public int EsporteId { get; set; }
        public virtual Esporte Esporte { get; set; }

        public virtual ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
    }
}