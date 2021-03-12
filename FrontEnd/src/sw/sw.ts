import { precacheAndRoute, matchPrecache } from "workbox-precaching";
import { registerRoute, setDefaultHandler } from "workbox-routing";
import {
  NetworkFirst,
  NetworkOnly,
  StaleWhileRevalidate,
} from "workbox-strategies";
import { CacheableResponsePlugin } from "workbox-cacheable-response";
import { ExpirationPlugin } from "workbox-expiration";

declare let self: ServiceWorkerGlobalScope;

self.addEventListener("message", (event) => {
  if (event.data && (event.data as { type: string }).type === "SKIP_WAITING") {
    void self.skipWaiting();
  }
});

precacheAndRoute(self.__WB_MANIFEST);

const networkOnly = new NetworkOnly();

registerRoute(new RegExp("/swagger/?.*"), new NetworkOnly());

registerRoute(new RegExp("/api/token/?.*"), new NetworkOnly());
registerRoute(new RegExp("/api/search/?.*"), new NetworkOnly());

registerRoute(
  new RegExp("/api/users/.+/avatar"),
  new StaleWhileRevalidate({
    cacheName: "avatars",
    plugins: [
      new CacheableResponsePlugin({
        statuses: [200],
      }),
      new ExpirationPlugin({
        maxAgeSeconds: 60 * 60 * 24 * 30 * 3, // 3 months
      }),
    ],
  })
);

registerRoute(
  new RegExp("/api/?.*"),
  new NetworkFirst({
    plugins: [
      new CacheableResponsePlugin({
        statuses: [200],
      }),
    ],
  })
);

setDefaultHandler((options) => {
  const { request } = options;

  if (request instanceof Request && request.destination === "document")
    return matchPrecache("/index.html").then((r) =>
      r == null ? Response.error() : r
    );
  else return networkOnly.handle(options);
});
