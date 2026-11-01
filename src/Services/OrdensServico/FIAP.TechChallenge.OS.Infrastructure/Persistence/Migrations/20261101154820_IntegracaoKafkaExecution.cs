using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntegracaoKafkaExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCancelamento",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDecisaoUtc",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEnvioAprovacao",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFinalizacao",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicioDiagnostico",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicioExecucao",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Decisao",
                table: "OrdensServico",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "OrdensServico",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "MensagensProcessadas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensProcessadas", x => x.Id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordens_Decisao",
                table: "OrdensServico",
                sql: "\"Decisao\" IN (0, 1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico",
                sql: "\"Status\" IN (1, 2, 3, 4, 5, 7)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensagensProcessadas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordens_Decisao",
                table: "OrdensServico");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DataCancelamento",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DataDecisaoUtc",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DataEnvioAprovacao",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DataFinalizacao",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DataInicioDiagnostico",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DataInicioExecucao",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "Decisao",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "OrdensServico");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico",
                sql: "\"Status\" = 1");
        }
    }
}
