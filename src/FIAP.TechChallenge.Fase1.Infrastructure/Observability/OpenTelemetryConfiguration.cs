using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Observability;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        _ = services.AddOpenTelemetry()
            .ConfigureResource(resource => ResourceBuilder.CreateDefault())
            .WithTracing(tracing =>
                tracing.AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter()
            )
            .WithMetrics(metrics =>
                metrics.AddAspNetCoreInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter()
            );

        return services;
    }

    public static ILoggingBuilder ConfigureOpenTelemetry(this ILoggingBuilder logging)
    {
        _ = logging.AddOpenTelemetry(options =>
        {
            _ = options.SetResourceBuilder(ResourceBuilder.CreateDefault());
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;

            _ = options.AddConsoleExporter();
            _ = options.AddOtlpExporter();
        });

        return logging;
    }
}
