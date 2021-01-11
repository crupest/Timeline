import { Observable } from "rxjs";

export type DataStatus = "syncing" | "synced" | "offline";

export function mergeDataStatus(statusList: DataStatus[]): DataStatus {
  if (statusList.includes("offline")) {
    return "offline";
  } else if (statusList.includes("syncing")) {
    return "syncing";
  } else {
    return "synced";
  }
}

export type Subscriber<TData> = (data: TData) => void;

export interface DataAndStatus<TData> {
  data: TData | null;
  status: DataStatus;
}

export class DataLine2<TData> {
  constructor(
    private config: {
      saveData: (data: TData) => Promise<void>;
      getSavedData: () => Promise<TData | null>;
      // return null for offline
      fetchData: (savedData: TData | null) => Promise<TData | null>;
    }
  ) {}

  private _current: DataAndStatus<TData> | null = null;
  private _observers: Subscriber<DataAndStatus<TData>>[] = [];

  get currentData(): DataAndStatus<TData> | null {
    return this._current;
  }

  get isDestroyable(): boolean {
    const { _observers, currentData } = this;
    return (
      _observers.length === 0 &&
      (currentData == null || currentData.status !== "syncing")
    );
  }

  private next(data: DataAndStatus<TData>): void {
    this._current = data;
    this._observers.forEach((o) => o(data));
  }

  subscribe(subsriber: Subscriber<DataAndStatus<TData>>): void {
    this.sync(); // TODO: Should I sync at this point or let the user sync explicitly.
    this._observers.push(subsriber);
    const { currentData } = this;
    if (currentData != null) {
      subsriber(currentData);
    }
  }

  unsubscribe(subsriber: Subscriber<DataAndStatus<TData>>): void {
    const index = this._observers.indexOf(subsriber);
    if (index > -1) this._observers.splice(index, 1);
  }

  getObservalble(): Observable<DataAndStatus<TData>> {
    return new Observable<DataAndStatus<TData>>((observer) => {
      const f = (data: DataAndStatus<TData>): void => {
        observer.next(data);
      };
      this.subscribe(f);

      return () => {
        this.unsubscribe(f);
      };
    });
  }

  sync(): void {
    const { currentData } = this;
    if (currentData != null && currentData.status === "syncing") return;
    this.next({ data: currentData?.data ?? null, status: "syncing" });
    void this.config.getSavedData().then((savedData) => {
      if (currentData == null && savedData != null) {
        this.next({ data: savedData, status: "syncing" });
      }
      return this.config.fetchData(savedData).then((data) => {
        if (data == null) {
          this.next({
            data: savedData,
            status: "offline",
          });
        } else {
          return this.config.saveData(data).then(() => {
            this.next({ data: data, status: "synced" });
          });
        }
      });
    });
  }

  save(data: TData): void {
    const { currentData } = this;
    if (currentData != null && currentData.status === "syncing") return;
    this.next({ data: currentData?.data ?? null, status: "syncing" });
    void this.config.saveData(data).then(() => {
      this.next({ data: data, status: "synced" });
    });
  }

  getSavedData(): Promise<TData | null> {
    return this.config.getSavedData();
  }
}

export class DataHub2<TKey, TData> {
  private readonly subscriptionLineMap = new Map<string, DataLine2<TData>>();

  private keyToString: (key: TKey) => string;

  private cleanTimerId = 0;

  // setup is called after creating line and if it returns a function as destroyer, then when the line is destroyed the destroyer will be called.
  constructor(
    private config: {
      saveData: (key: TKey, data: TData) => Promise<void>;
      getSavedData: (key: TKey) => Promise<TData | null>;
      fetchData: (key: TKey, savedData: TData | null) => Promise<TData | null>;
      keyToString?: (key: TKey) => string;
    }
  ) {
    this.keyToString =
      config.keyToString ??
      ((value): string => {
        if (typeof value === "string") return value;
        else
          throw new Error(
            "Default keyToString function only pass string value."
          );
      });
  }

  private cleanLines(): void {
    const toDelete: string[] = [];
    for (const [key, line] of this.subscriptionLineMap.entries()) {
      if (line.isDestroyable) {
        toDelete.push(key);
      }
    }

    if (toDelete.length === 0) return;

    for (const key of toDelete) {
      this.subscriptionLineMap.delete(key);
    }

    if (this.subscriptionLineMap.size === 0) {
      window.clearInterval(this.cleanTimerId);
      this.cleanTimerId = 0;
    }
  }

  private createLine(key: TKey): DataLine2<TData> {
    const keyString = this.keyToString(key);
    const newLine: DataLine2<TData> = new DataLine2<TData>({
      saveData: (data) => this.config.saveData(key, data),
      getSavedData: () => this.config.getSavedData(key),
      fetchData: (savedData) => this.config.fetchData(key, savedData),
    });
    this.subscriptionLineMap.set(keyString, newLine);
    if (this.subscriptionLineMap.size === 1) {
      this.cleanTimerId = window.setInterval(this.cleanLines.bind(this), 20000);
    }
    return newLine;
  }

  getLine(key: TKey): DataLine2<TData> {
    const keyString = this.keyToString(key);
    return this.subscriptionLineMap.get(keyString) ?? this.createLine(key);
  }
}
