using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jogos_Academicos.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoDominio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Curso_fk_curso",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Curso",
                table: "Curso");

            migrationBuilder.RenameTable(
                name: "Curso",
                newName: "cursos");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "cursos",
                newName: "id_cursos");

            migrationBuilder.AddColumn<int>(
                name: "fk_campus",
                table: "cursos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "nivel",
                table: "cursos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cursos",
                table: "cursos",
                column: "id_cursos");

            migrationBuilder.CreateTable(
                name: "campus",
                columns: table => new
                {
                    id_campus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Endereco = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campus", x => x.id_campus);
                });

            migrationBuilder.CreateTable(
                name: "esportes",
                columns: table => new
                {
                    id_esporte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_esportes", x => x.id_esporte);
                });

            migrationBuilder.CreateTable(
                name: "equipes",
                columns: table => new
                {
                    id_equipe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fk_tecnico = table.Column<int>(type: "int", nullable: true),
                    fk_esporte = table.Column<int>(type: "int", nullable: false),
                    fk_curso = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipes", x => x.id_equipe);
                    table.ForeignKey(
                        name: "FK_equipes_Usuarios_fk_tecnico",
                        column: x => x.fk_tecnico,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK_equipes_cursos_fk_curso",
                        column: x => x.fk_curso,
                        principalTable: "cursos",
                        principalColumn: "id_cursos",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_equipes_esportes_fk_esporte",
                        column: x => x.fk_esporte,
                        principalTable: "esportes",
                        principalColumn: "id_esporte",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "eventos",
                columns: table => new
                {
                    id_eventos = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nivel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fk_esporte = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos", x => x.id_eventos);
                    table.ForeignKey(
                        name: "FK_eventos_esportes_fk_esporte",
                        column: x => x.fk_esporte,
                        principalTable: "esportes",
                        principalColumn: "id_esporte",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipe_atleta",
                columns: table => new
                {
                    AtletasId = table.Column<int>(type: "int", nullable: false),
                    EquipeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipe_atleta", x => new { x.AtletasId, x.EquipeId });
                    table.ForeignKey(
                        name: "FK_equipe_atleta_Usuarios_AtletasId",
                        column: x => x.AtletasId,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_equipe_atleta_equipes_EquipeId",
                        column: x => x.EquipeId,
                        principalTable: "equipes",
                        principalColumn: "id_equipe",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grupos",
                columns: table => new
                {
                    id_grupo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fk_eventos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos", x => x.id_grupo);
                    table.ForeignKey(
                        name: "FK_grupos_eventos_fk_eventos",
                        column: x => x.fk_eventos,
                        principalTable: "eventos",
                        principalColumn: "id_eventos",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grupos_equipes",
                columns: table => new
                {
                    EquipesId = table.Column<int>(type: "int", nullable: false),
                    GruposId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos_equipes", x => new { x.EquipesId, x.GruposId });
                    table.ForeignKey(
                        name: "FK_grupos_equipes_equipes_EquipesId",
                        column: x => x.EquipesId,
                        principalTable: "equipes",
                        principalColumn: "id_equipe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grupos_equipes_grupos_GruposId",
                        column: x => x.GruposId,
                        principalTable: "grupos",
                        principalColumn: "id_grupo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cursos_fk_campus",
                table: "cursos",
                column: "fk_campus");

            migrationBuilder.CreateIndex(
                name: "IX_equipe_atleta_EquipeId",
                table: "equipe_atleta",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_equipes_fk_curso",
                table: "equipes",
                column: "fk_curso");

            migrationBuilder.CreateIndex(
                name: "IX_equipes_fk_esporte",
                table: "equipes",
                column: "fk_esporte");

            migrationBuilder.CreateIndex(
                name: "IX_equipes_fk_tecnico",
                table: "equipes",
                column: "fk_tecnico");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_fk_esporte",
                table: "eventos",
                column: "fk_esporte");

            migrationBuilder.CreateIndex(
                name: "IX_grupos_fk_eventos",
                table: "grupos",
                column: "fk_eventos");

            migrationBuilder.CreateIndex(
                name: "IX_grupos_equipes_GruposId",
                table: "grupos_equipes",
                column: "GruposId");

            migrationBuilder.AddForeignKey(
                name: "FK_cursos_campus_fk_campus",
                table: "cursos",
                column: "fk_campus",
                principalTable: "campus",
                principalColumn: "id_campus",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_cursos_fk_curso",
                table: "Usuarios",
                column: "fk_curso",
                principalTable: "cursos",
                principalColumn: "id_cursos",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cursos_campus_fk_campus",
                table: "cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_cursos_fk_curso",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "campus");

            migrationBuilder.DropTable(
                name: "equipe_atleta");

            migrationBuilder.DropTable(
                name: "grupos_equipes");

            migrationBuilder.DropTable(
                name: "equipes");

            migrationBuilder.DropTable(
                name: "grupos");

            migrationBuilder.DropTable(
                name: "eventos");

            migrationBuilder.DropTable(
                name: "esportes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cursos",
                table: "cursos");

            migrationBuilder.DropIndex(
                name: "IX_cursos_fk_campus",
                table: "cursos");

            migrationBuilder.DropColumn(
                name: "fk_campus",
                table: "cursos");

            migrationBuilder.DropColumn(
                name: "nivel",
                table: "cursos");

            migrationBuilder.RenameTable(
                name: "cursos",
                newName: "Curso");

            migrationBuilder.RenameColumn(
                name: "id_cursos",
                table: "Curso",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Curso",
                table: "Curso",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Curso_fk_curso",
                table: "Usuarios",
                column: "fk_curso",
                principalTable: "Curso",
                principalColumn: "Id");
        }
    }
}
