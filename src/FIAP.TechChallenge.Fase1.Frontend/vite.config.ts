import path from "node:path";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
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
});
