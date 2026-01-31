using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Jogos_Academicos.Models.Enums;

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

        public FaseTorneio Fase { get; set; }
        public int NumeroRodada { get; set; }

        // --- RELACIONAMENTOS ---

        public int ArbitroId { get; set; }
        [ForeignKey("ArbitroId")]
        public virtual Usuario Arbitro { get; set; }

        public int EquipeAId { get; set; }
        [ForeignKey("EquipeAId")]
        public virtual Equipe EquipeA { get; set; }

        public int EquipeBId { get; set; }
        [ForeignKey("EquipeBId")]
        public virtual Equipe EquipeB { get; set; }

        // Grupo é Opcional (Nullable)
        public int? GrupoId { get; set; }
        [ForeignKey("GrupoId")] // Adicionei esta anotação para garantir
        public virtual Grupo Grupo { get; set; }

        // CORREÇÃO PRINCIPAL AQUI:
        public int EventoId { get; set; }

        [ForeignKey("EventoId")] // <--- ESTA ANOTAÇÃO CORRIGE O ERRO 'EventoId1'
        public virtual Evento Evento { get; set; }
    }
}