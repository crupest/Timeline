/**
 * @type {import('vite').UserConfig}
 */

import reactRefresh from "@vitejs/plugin-react-refresh";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [reactRefresh()],
  resolve: {
    alias: [{ find: "@", replacement: "/src" }],
  },
  server: {
    port: 13000,
    proxy: {
      "/api": {
        target: "http://localhost:5000",
        changeOrigin: true,
      },
    },
  },
});
