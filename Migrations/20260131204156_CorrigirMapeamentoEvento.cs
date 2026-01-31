using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jogos_Academicos.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirMapeamentoEvento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jogos_eventos_EventoId",
                table: "jogos");

            migrationBuilder.DropForeignKey(
                name: "FK_jogos_grupos_GrupoId",
                table: "jogos");

            migrationBuilder.AddForeignKey(
                name: "FK_jogos_eventos_EventoId",
                table: "jogos",
                column: "EventoId",
                principalTable: "eventos",
                principalColumn: "id_eventos");

            migrationBuilder.AddForeignKey(
                name: "FK_jogos_grupos_GrupoId",
                table: "jogos",
                column: "GrupoId",
                principalTable: "grupos",
                principalColumn: "id_grupo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jogos_eventos_EventoId",
                table: "jogos");

            migrationBuilder.DropForeignKey(
                name: "FK_jogos_grupos_GrupoId",
                table: "jogos");

            migrationBuilder.AddForeignKey(
                name: "FK_jogos_eventos_EventoId",
                table: "jogos",
                column: "EventoId",
                principalTable: "eventos",
                principalColumn: "id_eventos",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_jogos_grupos_GrupoId",
                table: "jogos",
                column: "GrupoId",
                principalTable: "grupos",
                principalColumn: "id_grupo",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
