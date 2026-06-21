using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.Fase1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntidadesDeOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PecasOuInsumoDaOrdemDeServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PecaInsumoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PecasOuInsumoDaOrdemDeServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PecasOuInsumoDaOrdemDeServico_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicosDaOrdemDeServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicosDaOrdemDeServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicosDaOrdemDeServico_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PecasOuInsumoDaOrdemDeServico_OrdemServicoId",
                table: "PecasOuInsumoDaOrdemDeServico",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicosDaOrdemDeServico_OrdemServicoId",
                table: "ServicosDaOrdemDeServico",
                column: "OrdemServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PecasOuInsumoDaOrdemDeServico");

            migrationBuilder.DropTable(
                name: "ServicosDaOrdemDeServico");
        }
    }
}

