import path from "node:path";
import { createLogger, defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import { redactPrivateQuery } from "./src/observability/privacy";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const gateway = env.API_GATEWAY_UPSTREAM;
  const proxy = (target: string) => ({ target, changeOrigin: true });
  const logger = createLogger();
  const logError = logger.error.bind(logger);
  logger.error = (message, options) => logError(redactPrivateQuery(message), options);
  const logWarn = logger.warn.bind(logger);
  logger.warn = (message, options) => logWarn(redactPrivateQuery(message), options);
  return {
    customLogger: logger,
    plugins: [react()],
    server: {
      proxy: {
        "/api/os/v1": proxy(gateway || env.OS_UPSTREAM || "http://localhost:8081"),
        "/api/execution/v1": proxy(gateway || env.EXECUTION_UPSTREAM || "http://localhost:8082"),
        "/api/billing/v1": proxy(gateway || env.BILLING_UPSTREAM || "http://localhost:8083"),
        "/otlp": {
          target: "http://localhost:4318",
          changeOrigin: true,
          rewrite: (requestPath) => requestPath.replace(/^\/otlp/, ""),
        },
      },
    },
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
    },
  };
});
