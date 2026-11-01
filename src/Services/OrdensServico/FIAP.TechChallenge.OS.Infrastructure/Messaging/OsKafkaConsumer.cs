using Confluent.Kafka;
using FIAP.TechChallenge.Execution.Application.Messaging;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarDiagnosticoConcluido;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarDiagnosticoIniciado;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoCancelada;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoConcluida;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoIniciada;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace FIAP.TechChallenge.OS.Infrastructure.Messaging;

public sealed class OsKafkaConsumer(IConfiguration configuration, IServiceScopeFactory scopes,
    ConsumerState state, ILogger<OsKafkaConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"] ?? "oficina-os-v1",
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            AllowAutoCreateTopics = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        }).Build();
        ConsumeResult<string, string>? record = null;
        Guid? messageId = null;
        string? correlationId = null;
        try
        {
            consumer.Subscribe(configuration["Kafka:InputTopic"] ?? ExecutionMessaging.Topic);
            state.Running = true;
            while (!stoppingToken.IsCancellationRequested)
            {
                record = consumer.Consume(stoppingToken);
                messageId = null;
                correlationId = null;
                if (record.IsPartitionEOF)
                    continue;
                var envelope = JsonSerializer.Deserialize<MessageEnvelope<JsonElement>>(record.Message.Value, JsonOptions)
                    ?? throw new InvalidOperationException("Envelope ausente.");
                messageId = envelope.MessageId;
                correlationId = envelope.CorrelationId;
                if (envelope.MessageId == Guid.Empty || envelope.Version != ExecutionMessaging.Version
                    || envelope.OccurredAtUtc == default || envelope.OccurredAtUtc.Offset != TimeSpan.Zero
                    || !ValidCorrelation(envelope.CorrelationId))
                    throw new InvalidOperationException("Metadados de mensagem inválidos.");
                if (!Guid.TryParseExact(record.Message.Key, "D", out var key) || key == Guid.Empty)
                    throw new InvalidOperationException("Chave Kafka inválida.");
                using var logScope = logger.BeginScope(new Dictionary<string, object>
                {
                    ["MessageId"] = envelope.MessageId,
                    ["CorrelationId"] = envelope.CorrelationId,
                    ["Topic"] = record.Topic,
                    ["Partition"] = record.Partition.Value,
                    ["Offset"] = record.Offset.Value
                });
                using var activity = new Activity("os.kafka.consume").SetIdFormat(ActivityIdFormat.W3C);
                if (envelope.TraceParent is not null)
                    _ = activity.SetParentId(envelope.TraceParent);
                activity.TraceStateString = envelope.TraceState;
                _ = activity.AddBaggage("correlation.id", envelope.CorrelationId);
                _ = activity.Start();
                await using var scope = scopes.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<MessageContext>();
                context.IncomingMessageId = envelope.MessageId;
                context.CorrelationId = envelope.CorrelationId;
                Result<bool> result;
                switch (envelope.Type, envelope.Version)
                {
                    case (DiagnosticoIniciado.Type, ExecutionMessaging.Version):
                        {
                            var payload = envelope.Payload.Deserialize<DiagnosticoIniciado>(JsonOptions)
                                ?? throw new InvalidOperationException("Payload ausente.");
                            if (payload.OrdemServicoId != key)
                                throw new InvalidOperationException("Chave diferente da ordem do payload.");
                            result = await scope.ServiceProvider.GetRequiredService<IRegistrarDiagnosticoIniciadoUseCase>()
                                .ExecuteAsync(new(payload.OrdemServicoId, envelope.OccurredAtUtc), stoppingToken);
                            break;
                        }
                    case (DiagnosticoConcluido.Type, ExecutionMessaging.Version):
                        {
                            var payload = envelope.Payload.Deserialize<DiagnosticoConcluido>(JsonOptions)
                                ?? throw new InvalidOperationException("Payload ausente.");
                            if (payload.OrdemServicoId != key)
                                throw new InvalidOperationException("Chave diferente da ordem do payload.");
                            if (payload.Servicos is null || payload.PecasInsumos is null)
                                throw new InvalidOperationException("Itens do diagnóstico ausentes.");
                            result = await scope.ServiceProvider.GetRequiredService<IRegistrarDiagnosticoConcluidoUseCase>()
                                .ExecuteAsync(new(payload.OrdemServicoId, envelope.OccurredAtUtc), stoppingToken);
                            break;
                        }
                    case (ExecucaoIniciada.Type, ExecutionMessaging.Version):
                        {
                            var payload = envelope.Payload.Deserialize<ExecucaoIniciada>(JsonOptions)
                                ?? throw new InvalidOperationException("Payload ausente.");
                            if (payload.OrdemServicoId != key)
                                throw new InvalidOperationException("Chave diferente da ordem do payload.");
                            result = await scope.ServiceProvider.GetRequiredService<IRegistrarExecucaoIniciadaUseCase>()
                                .ExecuteAsync(new(payload.OrdemServicoId, envelope.OccurredAtUtc), stoppingToken);
                            break;
                        }
                    case (ExecucaoConcluida.Type, ExecutionMessaging.Version):
                        {
                            var payload = envelope.Payload.Deserialize<ExecucaoConcluida>(JsonOptions)
                                ?? throw new InvalidOperationException("Payload ausente.");
                            if (payload.OrdemServicoId != key)
                                throw new InvalidOperationException("Chave diferente da ordem do payload.");
                            result = await scope.ServiceProvider.GetRequiredService<IRegistrarExecucaoConcluidaUseCase>()
                                .ExecuteAsync(new(payload.OrdemServicoId, envelope.OccurredAtUtc), stoppingToken);
                            break;
                        }
                    case (ExecucaoCancelada.Type, ExecutionMessaging.Version):
                        {
                            var payload = envelope.Payload.Deserialize<ExecucaoCancelada>(JsonOptions)
                                ?? throw new InvalidOperationException("Payload ausente.");
                            if (payload.OrdemServicoId != key)
                                throw new InvalidOperationException("Chave diferente da ordem do payload.");
                            result = await scope.ServiceProvider.GetRequiredService<IRegistrarExecucaoCanceladaUseCase>()
                                .ExecuteAsync(new(payload.OrdemServicoId, envelope.OccurredAtUtc), stoppingToken);
                            break;
                        }
                    default:
                        throw new InvalidOperationException("Tipo ou versão de mensagem não suportado.");
                }
                if (!result.IsSuccess)
                    throw new InvalidOperationException($"Caso de uso recusou a mensagem: {result.Error.Description}");
                consumer.Commit(record);
                logger.LogInformation("Mensagem processada e offset confirmado.");
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Consumo interrompido sem avançar offset. Tópico {Topic}, partição {Partition}, offset {Offset}, mensagem {MessageId}, correlação {CorrelationId}. Corrija e reinicie.",
                record?.Topic, record?.Partition.Value, record?.Offset.Value, messageId, correlationId);
            Environment.ExitCode = 1;
            throw;
        }
        finally
        {
            state.Running = false;
            try
            { consumer.Close(); }
            catch (KafkaException exception) { logger.LogError(exception, "Falha ao fechar o consumidor."); }
        }
    }
    private static bool ValidCorrelation(string? value) => value is { Length: > 0 and <= 128 }
        && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-');
}
