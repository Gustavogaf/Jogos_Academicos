using Jogos_Academicos.Models;
using Jogos_Academicos.Models.enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jogos_Academicos.Models
{
    [Table("Usuarios")] // Mantendo convenção do Java @Table(name="usuarios")
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [Column("nome_completo")]
        public string NomeCompleto { get; set; }

        [Column("apelido")]
        public string Apelido { get; set; }

        [Column("telefone")]
        public string Telefone { get; set; }

        [Required]
        [Column("matricula")]
        public string Matricula { get; set; }

        [Required]
        [Column("email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Column("password")] // Mapeando para o nome da coluna no banco legado
        public string Senha { get; set; }

        [Column("tipo_usuario")]
        public Role TipoUsuario { get; set; }

        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // --- Relacionamentos (Stubs para compilar) ---
        // O Dev 02 irá implementar as classes reais depois.

        [Column("fk_curso")]
        public int? CursoId { get; set; }
        public Curso Curso { get; set; }

        // Coleções (Ignore por enquanto ou use ICollection vazia)
    }
}
