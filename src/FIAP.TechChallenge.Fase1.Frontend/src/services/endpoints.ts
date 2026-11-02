type Environment = Record<string, string | undefined>;

// Bases completas podem ser sobrescritas para gateways com stage/prefixo próprio.
export function resolveApiEndpoints(env: Environment) {
  const gateway = (env.VITE_API_GATEWAY_URL || "").replace(/\/+$/, "");
  return {
    os: env.VITE_OS_API_BASE_URL || `${gateway}/api/os/v1`,
    execution: env.VITE_EXECUTION_API_BASE_URL || `${gateway}/api/execution/v1`,
    billing: env.VITE_BILLING_API_BASE_URL || `${gateway}/api/billing/v1`,
  };
}
