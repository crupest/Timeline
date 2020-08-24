import localforage from "localforage";

import { HttpNetworkError } from "../http/common";

export const dataStorage = localforage.createInstance({
  name: "data",
  description: "Database for offline data.",
  driver: localforage.INDEXEDDB,
});

export class ForbiddenError extends Error {
  constructor(message?: string) {
    super(message);
  }
}

export function throwIfNotNetworkError(e: unknown): void {
  if (!(e instanceof HttpNetworkError)) {
    throw e;
  }
}

export type BlobOrStatus = Blob | "loading" | "error";
