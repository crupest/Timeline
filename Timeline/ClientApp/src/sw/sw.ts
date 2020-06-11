import { precacheAndRoute, matchPrecache } from 'workbox-precaching';
import { setDefaultHandler } from 'workbox-routing';
import { NetworkOnly } from 'workbox-strategies';

declare let self: ServiceWorkerGlobalScope;

self.addEventListener('message', (event) => {
  if (event.data && (event.data as { type: string }).type === 'SKIP_WAITING') {
    void self.skipWaiting();
  }
});

precacheAndRoute(self.__WB_MANIFEST);

const networkOnly = new NetworkOnly();

setDefaultHandler((options) => {
  const { request, url } = options;
  if (url && url.pathname.startsWith('/api/')) {
    return networkOnly.handle(options);
  }

  if (request instanceof Request && request.destination === 'document')
    return matchPrecache('/index.html').then((r) =>
      r == null ? Response.error() : r
    );
  else return networkOnly.handle(options);
});
