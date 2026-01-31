using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Jogos_Academicos.Models.Enums; // Se Fase for um Enum

namespace Jogos_Academicos.Models
{
    [Table("jogos")]
    public class Jogo
    {
        [Key]
        [Column("id_jogo")]
        public int Id { get; set; }

        public int? PlacarA { get; set; }
        public int? PlacarB { get; set; }

        public bool WoA { get; set; }
        public bool WoB { get; set; }
        public bool Finalizado { get; set; }

        public DateTime DataHora { get; set; }

        // Relacionamentos
        public int ArbitroId { get; set; }
        [ForeignKey("ArbitroId")]
        public virtual Usuario Arbitro { get; set; }

        public int EquipeAId { get; set; }
        [ForeignKey("EquipeAId")]
        public virtual Equipe EquipeA { get; set; }

        public int EquipeBId { get; set; }
        [ForeignKey("EquipeBId")]
        public virtual Equipe EquipeB { get; set; }

        public int GrupoId { get; set; }
        public virtual Grupo Grupo { get; set; }

        public int EventoId { get; set; }
        public virtual Evento Evento { get; set; }
    }
}
