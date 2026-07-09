using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Observability;

public static class OpenTelemetryConfiguration
{
    public static IHostApplicationBuilder AddOpenTelemetry(this IHostApplicationBuilder builder)
    {
        _ = builder.Services.ConfigureServices();
        _ = builder.Logging.ConfigureLogging();
        return builder;
    }

    private static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        _ = services.AddOpenTelemetry()
            .ConfigureResource(resource => ResourceBuilder.CreateDefault())
            .WithTracing(tracing =>
                tracing.AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter()
            )
            .WithMetrics(metrics =>
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter()
            );

        return services;
    }

    private static ILoggingBuilder ConfigureLogging(this ILoggingBuilder logging)
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
