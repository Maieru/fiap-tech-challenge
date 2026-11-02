using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BillingSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrcamentoId",
                table: "OrdensServico",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PagamentoJson",
                table: "OrdensServico",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorOrcamento",
                table: "OrdensServico",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrcamentoId",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "PagamentoJson",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "ValorOrcamento",
                table: "OrdensServico");
        }
    }
}
