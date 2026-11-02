using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EntregaEAcompanhamentoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEntrega",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrcamentoJson",
                table: "OrdensServico",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcessosCliente",
                columns: table => new
                {
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcessosCliente", x => x.OrdemServicoId);
                    table.ForeignKey(
                        name: "FK_AcessosCliente_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico",
                sql: "\"Status\" IN (1, 2, 3, 4, 5, 6, 7)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcessosCliente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DataEntrega",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "OrcamentoJson",
                table: "OrdensServico");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico",
                sql: "\"Status\" IN (1, 2, 3, 4, 5, 7)");
        }
    }
}
