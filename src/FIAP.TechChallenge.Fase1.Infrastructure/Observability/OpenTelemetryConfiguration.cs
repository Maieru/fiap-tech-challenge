using FIAP.TechChallenge.Fase1.Domain.Observability;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Observability;

public static class OpenTelemetryConfiguration
{
    public static IHostApplicationBuilder AddOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.AddChassis(new ChassisOptions
        {
            ServiceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "fiap-tech-challenge-backend",
            EnableOtlp = true,
            MeterNames = [MetricasNegocio.MeterName]
        });
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddEntityFrameworkCoreInstrumentation())
            .WithMetrics(metrics => metrics.AddProcessInstrumentation());
        return builder;
    }

}
