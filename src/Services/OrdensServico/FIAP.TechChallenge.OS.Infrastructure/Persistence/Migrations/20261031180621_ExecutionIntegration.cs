using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico");

            migrationBuilder.AddColumn<string>(
                name: "AcaoPendente",
                table: "OrdensServico",
                type: "text",
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

            migrationBuilder.AddColumn<string>(
                name: "DiagnosticoJson",
                table: "OrdensServico",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InputSequence",
                table: "OrdensServico",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "OutputSequence",
                table: "OrdensServico",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<Guid>(
                name: "SolicitacaoPendenteId",
                table: "OrdensServico",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "OrdensServico",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Detalhes",
                table: "HistoricoOrdens",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DispatcherLeases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Owner = table.Column<string>(type: "text", nullable: false),
                    UntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatcherLeases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Outbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    Body = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Published = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outbox", x => x.Id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico",
                sql: "\"Status\" IN (1,2,3,4,5,6,7)");

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_OrdemServicoId_Sequence",
                table: "Outbox",
                columns: new[] { "OrdemServicoId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_Published_Sequence_CreatedAtUtc",
                table: "Outbox",
                columns: new[] { "Published", "Sequence", "CreatedAtUtc" });
            migrationBuilder.Sql(OutboxBackfill.Sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DO $$ BEGIN IF EXISTS (SELECT 1 FROM \"OrdensServico\" WHERE \"Status\" <> 1 OR \"AcaoPendente\" IS NOT NULL) THEN RAISE EXCEPTION 'Rollback requires a forward-compatible data migration after workflow activity.'; END IF; END $$;");
            migrationBuilder.DropTable(
                name: "DispatcherLeases");

            migrationBuilder.DropTable(
                name: "Inbox");

            migrationBuilder.DropTable(
                name: "Outbox");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "AcaoPendente",
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
                name: "DiagnosticoJson",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "InputSequence",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "OutputSequence",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "SolicitacaoPendenteId",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "Detalhes",
                table: "HistoricoOrdens");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordens_Status",
                table: "OrdensServico",
                sql: "\"Status\" = 1");
        }
    }
}
