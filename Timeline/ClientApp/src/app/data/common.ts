import localforage from 'localforage';

export const dataStorage = localforage.createInstance({
  name: 'data',
  description: 'Database for offline data.',
  driver: localforage.INDEXEDDB,
});

export interface BlobWithUrl {
  blob: Blob;
  url: string;
}
