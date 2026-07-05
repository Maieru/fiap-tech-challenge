using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.Fase1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoAprovacaoOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CodigoAprovacao",
                table: "OrdensServico",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_CodigoAprovacao",
                table: "OrdensServico",
                column: "CodigoAprovacao",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrdensServico_CodigoAprovacao",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "CodigoAprovacao",
                table: "OrdensServico");
        }
    }
}
