/**
 * @type {import('vite').UserConfig}
 */

import reactRefresh from "@vitejs/plugin-react-refresh";
import { VitePWA } from "vite-plugin-pwa";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [
    reactRefresh(),
    VitePWA({
      strategies: "injectManifest",
      srcDir: "src",
      filename: "sw.ts",
      base: "/",
      manifest: false,
      includeAssets: "**",
    }),
  ],
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
