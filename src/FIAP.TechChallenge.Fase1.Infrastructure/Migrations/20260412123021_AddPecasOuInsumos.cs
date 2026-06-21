using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.Fase1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPecasOuInsumos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PecasInsumos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    QuantidadeEstoque = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PecasInsumos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PecasInsumos");
        }
    }
}

