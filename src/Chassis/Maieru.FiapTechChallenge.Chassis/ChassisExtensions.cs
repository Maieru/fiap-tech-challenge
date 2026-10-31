using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Maieru.FiapTechChallenge.Chassis;

public static class ChassisExtensions
{
    public const string CorrelationHeader = "X-Correlation-ID";

    public static IHostApplicationBuilder AddChassis(this IHostApplicationBuilder builder, ChassisOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ServiceName);
        builder.Services.AddSingleton(options);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationPropagationHandler>();
        builder.Services.ConfigureHttpClientDefaults(client => client.AddHttpMessageHandler<CorrelationPropagationHandler>());
        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(console => console.IncludeScopes = true);
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(options.ServiceName, serviceVersion: options.ServiceVersion));
            logging.IncludeScopes = true;
            logging.IncludeFormattedMessage = true;
            logging.ParseStateValues = true;
            if (options.EnableOtlp) logging.AddOtlpExporter();
        });
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName, serviceVersion: options.ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddSource(options.ActivitySourceNames);
                if (options.EnableOtlp) tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation().AddMeter(options.MeterNames);
                if (options.EnableOtlp) metrics.AddOtlpExporter();
            });
        return builder;
    }

    public static IApplicationBuilder UseChassis(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var supplied = context.Request.Headers[CorrelationHeader].ToString();
            var correlation = IsValidCorrelationId(supplied) ? supplied : Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
            context.Items[CorrelationHeader] = correlation;
            Activity.Current?.SetBaggage("correlation.id", correlation);
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationHeader] = correlation;
                return Task.CompletedTask;
            });
            using var scope = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Chassis.Http")
                .BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = correlation, ["TraceId"] = Activity.Current?.TraceId.ToString() });
            await next(context);
        });
        app.UseExceptionHandler(handler => handler.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var problem = new ProblemDetails { Status = 500, Title = "Ocorreu uma falha interna.", Instance = context.Request.Path };
            problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            problem.Extensions["correlationId"] = context.Items[CorrelationHeader];
            await context.Response.WriteAsJsonAsync(problem, options: (System.Text.Json.JsonSerializerOptions?)null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
        }));
        return app;
    }

    public static WebApplication MapChassisHealthChecks(this WebApplication app, string prefix = "/health")
    {
        app.MapHealthChecks($"{prefix}/live", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live"), ResponseWriter = WriteHealthAsync }).AllowAnonymous();
        app.MapHealthChecks($"{prefix}/ready", new HealthCheckOptions { Predicate = _ => true, ResponseWriter = WriteHealthAsync }).AllowAnonymous();
        return app;
    }

    private static Task WriteHealthAsync(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            dependencies = report.Entries.Select(entry => new { name = entry.Key, status = entry.Value.Status.ToString() })
        }, context.RequestAborted);

    private static bool IsValidCorrelationId(string value) => value.Length is > 0 and <= 128 && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');
}

internal sealed class CorrelationPropagationHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (accessor.HttpContext?.Items[ChassisExtensions.CorrelationHeader] is string correlation)
        {
            request.Headers.Remove(ChassisExtensions.CorrelationHeader);
            request.Headers.TryAddWithoutValidation(ChassisExtensions.CorrelationHeader, correlation);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
