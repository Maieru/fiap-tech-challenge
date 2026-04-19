using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.Fase1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampoTempoGastoEmServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Concluido",
                table: "ServicosDaOrdemDeServico",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TempoGastoMinutos",
                table: "ServicosDaOrdemDeServico",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concluido",
                table: "ServicosDaOrdemDeServico");

            migrationBuilder.DropColumn(
                name: "TempoGastoMinutos",
                table: "ServicosDaOrdemDeServico");
        }
    }
}
