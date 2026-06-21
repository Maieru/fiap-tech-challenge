using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.Fase1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_ClienteId",
                table: "Veiculos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_ClienteId",
                table: "OrdensServico",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_VeiculoId",
                table: "OrdensServico",
                column: "VeiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensServico_Clientes_ClienteId",
                table: "OrdensServico",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensServico_Veiculos_VeiculoId",
                table: "OrdensServico",
                column: "VeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Veiculos_Clientes_ClienteId",
                table: "Veiculos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdensServico_Clientes_ClienteId",
                table: "OrdensServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensServico_Veiculos_VeiculoId",
                table: "OrdensServico");

            migrationBuilder.DropForeignKey(
                name: "FK_Veiculos_Clientes_ClienteId",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Veiculos_ClienteId",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_OrdensServico_ClienteId",
                table: "OrdensServico");

            migrationBuilder.DropIndex(
                name: "IX_OrdensServico_VeiculoId",
                table: "OrdensServico");
        }
    }
}

