using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jogos_Academicos.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoJogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jogos",
                columns: table => new
                {
                    id_jogo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlacarA = table.Column<int>(type: "int", nullable: true),
                    PlacarB = table.Column<int>(type: "int", nullable: true),
                    WoA = table.Column<bool>(type: "bit", nullable: false),
                    WoB = table.Column<bool>(type: "bit", nullable: false),
                    Finalizado = table.Column<bool>(type: "bit", nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArbitroId = table.Column<int>(type: "int", nullable: false),
                    EquipeAId = table.Column<int>(type: "int", nullable: false),
                    EquipeBId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    EventoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jogos", x => x.id_jogo);
                    table.ForeignKey(
                        name: "FK_jogos_Usuarios_ArbitroId",
                        column: x => x.ArbitroId,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_jogos_equipes_EquipeAId",
                        column: x => x.EquipeAId,
                        principalTable: "equipes",
                        principalColumn: "id_equipe",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_jogos_equipes_EquipeBId",
                        column: x => x.EquipeBId,
                        principalTable: "equipes",
                        principalColumn: "id_equipe",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_jogos_eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "eventos",
                        principalColumn: "id_eventos",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_jogos_grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "grupos",
                        principalColumn: "id_grupo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_jogos_ArbitroId",
                table: "jogos",
                column: "ArbitroId");

            migrationBuilder.CreateIndex(
                name: "IX_jogos_EquipeAId",
                table: "jogos",
                column: "EquipeAId");

            migrationBuilder.CreateIndex(
                name: "IX_jogos_EquipeBId",
                table: "jogos",
                column: "EquipeBId");

            migrationBuilder.CreateIndex(
                name: "IX_jogos_EventoId",
                table: "jogos",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_jogos_GrupoId",
                table: "jogos",
                column: "GrupoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jogos");
        }
    }
}
