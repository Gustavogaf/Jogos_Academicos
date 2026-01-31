using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jogos_Academicos.Migrations
{
    /// <inheritdoc />
    public partial class TabelaClassificacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classificacao_grupo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pontos = table.Column<int>(type: "int", nullable: false),
                    Vitorias = table.Column<int>(type: "int", nullable: false),
                    Empates = table.Column<int>(type: "int", nullable: false),
                    Derrotas = table.Column<int>(type: "int", nullable: false),
                    SaldoGols = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    EquipeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classificacao_grupo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_classificacao_grupo_equipes_EquipeId",
                        column: x => x.EquipeId,
                        principalTable: "equipes",
                        principalColumn: "id_equipe",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_classificacao_grupo_grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "grupos",
                        principalColumn: "id_grupo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_classificacao_grupo_EquipeId",
                table: "classificacao_grupo",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_classificacao_grupo_GrupoId_EquipeId",
                table: "classificacao_grupo",
                columns: new[] { "GrupoId", "EquipeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "classificacao_grupo");
        }
    }
}
