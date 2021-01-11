import localforage from "localforage";

const dataVersion = 1;

export const dataStorage = localforage.createInstance({
  name: "data",
  description: "Database for offline data.",
  driver: localforage.INDEXEDDB,
});

void (async () => {
  const currentVersion = await dataStorage.getItem<number | null>("version");
  if (currentVersion !== dataVersion) {
    console.log("Data storage version has changed. Clear all data.");
    await dataStorage.clear();
    await dataStorage.setItem("version", dataVersion);
  }
})();

export class ForbiddenError extends Error {
  constructor(message?: string) {
    super(message);
  }
}
