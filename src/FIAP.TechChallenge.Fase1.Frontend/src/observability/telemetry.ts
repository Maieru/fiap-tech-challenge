import { SpanStatusCode, trace } from "@opentelemetry/api";
import { redactPrivateQuery } from "./privacy";
import { SeverityNumber } from "@opentelemetry/api-logs";
import { OTLPLogExporter } from "@opentelemetry/exporter-logs-otlp-http";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { registerInstrumentations } from "@opentelemetry/instrumentation";
import { DocumentLoadInstrumentation } from "@opentelemetry/instrumentation-document-load";
import { FetchInstrumentation } from "@opentelemetry/instrumentation-fetch";
import { XMLHttpRequestInstrumentation } from "@opentelemetry/instrumentation-xml-http-request";
import { resourceFromAttributes } from "@opentelemetry/resources";
import { BatchLogRecordProcessor, LoggerProvider } from "@opentelemetry/sdk-logs";
import { BatchSpanProcessor, WebTracerProvider } from "@opentelemetry/sdk-trace-web";
import {
  ATTR_DEPLOYMENT_ENVIRONMENT_NAME,
  ATTR_SERVICE_NAME,
  ATTR_SERVICE_VERSION,
} from "@opentelemetry/semantic-conventions";

// Links privados não iniciam instrumentação. Chamadas públicas também são ignoradas
// quando a página é acessada por navegação SPA a partir da área administrativa.
function initializeTelemetry() {
  if (window.location.pathname.startsWith("/acompanhar-ordem/")) return;
  const privateUrlPattern = /\/ordens\/acompanhamento\//;
  const serviceName = import.meta.env.VITE_OTEL_SERVICE_NAME || "fiap-tech-challenge-frontend";
  const exporterUrl = import.meta.env.VITE_OTEL_EXPORTER_URL || "/otlp/v1/traces";
  const logsExporterUrl = import.meta.env.VITE_OTEL_LOGS_EXPORTER_URL || "/otlp/v1/logs";
  const apiUrlPattern = /\/api(?:\/|$)/;
  const exporterUrlPattern = /\/otlp(?:\/|$)/;

  const resource = resourceFromAttributes({
    [ATTR_SERVICE_NAME]: serviceName,
    [ATTR_SERVICE_VERSION]: import.meta.env.VITE_APP_VERSION || "1.0.0",
    [ATTR_DEPLOYMENT_ENVIRONMENT_NAME]: import.meta.env.MODE,
  });

  const provider = new WebTracerProvider({
    resource,
    spanProcessors: [
      new BatchSpanProcessor(
        new OTLPTraceExporter({
          url: exporterUrl,
        }),
      ),
    ],
  });

  provider.register();

  const loggerProvider = new LoggerProvider({
    resource,
    processors: [
      new BatchLogRecordProcessor({
        exporter: new OTLPLogExporter({ url: logsExporterUrl }),
      }),
    ],
  });

  const logger = loggerProvider.getLogger(serviceName);

  logger.emit({
    severityNumber: SeverityNumber.INFO,
    severityText: "INFO",
    body: "OpenTelemetry inicializado no frontend",
    attributes: {
      "event.name": "frontend.telemetry.initialized",
      "browser.url": window.location.origin + window.location.pathname,
    },
  });

  registerInstrumentations({
    instrumentations: [
      new DocumentLoadInstrumentation(),
      new FetchInstrumentation({
        ignoreUrls: [exporterUrlPattern, privateUrlPattern],
        propagateTraceHeaderCorsUrls: [apiUrlPattern],
      }),
      new XMLHttpRequestInstrumentation({
        ignoreUrls: [exporterUrlPattern, privateUrlPattern],
        propagateTraceHeaderCorsUrls: [apiUrlPattern],
      }),
    ],
  });

  const tracer = trace.getTracer(serviceName);

  function recordUnhandledError(error: unknown, source: string) {
    if (window.location.pathname.startsWith("/acompanhar-ordem/")) return;
    const exception = new Error(redactPrivateQuery(error instanceof Error ? error.message : String(error)));
    if (error instanceof Error && error.stack) exception.stack = redactPrivateQuery(error.stack);
    const span = tracer.startSpan(source);

    span.recordException(exception);
    span.setStatus({ code: SpanStatusCode.ERROR, message: exception.message });
    span.end();
  }

  window.addEventListener("error", (event) => {
    recordUnhandledError(event.error ?? event.message, "browser.error");
  });

  window.addEventListener("unhandledrejection", (event) => {
    recordUnhandledError(event.reason, "browser.unhandled_rejection");
  });

}
initializeTelemetry();
