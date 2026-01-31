using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jogos_Academicos.Models
{
    [Table("classificacao_grupo")]
    public class ClassificacaoGrupo
    {
        [Key]
        public int Id { get; set; }

        public int Pontos { get; set; } = 0;
        public int Vitorias { get; set; } = 0;
        public int Empates { get; set; } = 0;
        public int Derrotas { get; set; } = 0;
        public int SaldoGols { get; set; } = 0;

        // Relacionamentos
        public int GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public virtual Grupo Grupo { get; set; }

        public int EquipeId { get; set; }
        [ForeignKey("EquipeId")]
        public virtual Equipe Equipe { get; set; }
    }
}
