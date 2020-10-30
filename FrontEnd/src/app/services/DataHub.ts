import { pull } from "lodash";
import { Observable, BehaviorSubject, combineLatest } from "rxjs";
import { map } from "rxjs/operators";

export type Subscriber<TData> = (data: TData) => void;

export type WithSyncStatus<T> = T & { syncing: boolean };

export class DataLine<TData> {
  private _current: TData | undefined = undefined;

  private _syncPromise: Promise<void> | null = null;
  private _syncingSubject = new BehaviorSubject<boolean>(false);

  private _observers: Subscriber<TData>[] = [];

  constructor(
    private config: {
      sync: () => Promise<void>;
      destroyable?: (value: TData | undefined) => boolean;
      disableInitSync?: boolean;
    }
  ) {
    if (config.disableInitSync !== true) {
      setTimeout(() => void this.sync());
    }
  }

  private subscribe(subscriber: Subscriber<TData>): void {
    this._observers.push(subscriber);
    if (this._current !== undefined) {
      subscriber(this._current);
    }
  }

  private unsubscribe(subscriber: Subscriber<TData>): void {
    if (!this._observers.includes(subscriber)) return;
    pull(this._observers, subscriber);
  }

  getObservable(): Observable<TData> {
    return new Observable<TData>((observer) => {
      const f = (data: TData): void => {
        observer.next(data);
      };
      this.subscribe(f);

      return () => {
        this.unsubscribe(f);
      };
    });
  }

  getSyncStatusObservable(): Observable<boolean> {
    return this._syncingSubject.asObservable();
  }

  getDataWithSyncStatusObservable(): Observable<WithSyncStatus<TData>> {
    return combineLatest([
      this.getObservable(),
      this.getSyncStatusObservable(),
    ]).pipe(
      map(([data, syncing]) => ({
        ...data,
        syncing,
      }))
    );
  }

  get value(): TData | undefined {
    return this._current;
  }

  next(value: TData): void {
    this._current = value;
    this._observers.forEach((observer) => observer(value));
  }

  get isSyncing(): boolean {
    return this._syncPromise != null;
  }

  sync(): Promise<void> {
    if (this._syncPromise == null) {
      this._syncingSubject.next(true);
      this._syncPromise = this.config.sync().then(() => {
        this._syncingSubject.next(false);
        this._syncPromise = null;
      });
    }

    return this._syncPromise;
  }

  syncWithAction(
    syncAction: (line: DataLine<TData>) => Promise<void>
  ): Promise<void> {
    if (this._syncPromise == null) {
      this._syncingSubject.next(true);
      this._syncPromise = syncAction(this).then(() => {
        this._syncingSubject.next(false);
        this._syncPromise = null;
      });
    }

    return this._syncPromise;
  }

  get destroyable(): boolean {
    const customDestroyable = this.config?.destroyable;

    return (
      this._observers.length === 0 &&
      !this.isSyncing &&
      (customDestroyable != null ? customDestroyable(this._current) : true)
    );
  }
}

export class DataHub<TKey, TData> {
  private sync: (key: TKey, line: DataLine<TData>) => Promise<void>;
  private keyToString: (key: TKey) => string;
  private destroyable?: (key: TKey, value: TData | undefined) => boolean;

  private readonly subscriptionLineMap = new Map<string, DataLine<TData>>();

  private cleanTimerId = 0;

  // setup is called after creating line and if it returns a function as destroyer, then when the line is destroyed the destroyer will be called.
  constructor(config: {
    sync: (key: TKey, line: DataLine<TData>) => Promise<void>;
    keyToString?: (key: TKey) => string;
    destroyable?: (key: TKey, value: TData | undefined) => boolean;
  }) {
    this.sync = config.sync;
    this.keyToString =
      config.keyToString ??
      ((value): string => {
        if (typeof value === "string") return value;
        else
          throw new Error(
            "Default keyToString function only pass string value."
          );
      });

    this.destroyable = config.destroyable;
  }

  private cleanLines(): void {
    const toDelete: string[] = [];
    for (const [key, line] of this.subscriptionLineMap.entries()) {
      if (line.destroyable) {
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

  private createLine(key: TKey, disableInitSync = false): DataLine<TData> {
    const keyString = this.keyToString(key);
    const { destroyable } = this;
    const newLine: DataLine<TData> = new DataLine<TData>({
      sync: () => this.sync(key, newLine),
      destroyable:
        destroyable != null ? (value) => destroyable(key, value) : undefined,
      disableInitSync: disableInitSync,
    });
    this.subscriptionLineMap.set(keyString, newLine);
    if (this.subscriptionLineMap.size === 1) {
      this.cleanTimerId = window.setInterval(this.cleanLines.bind(this), 20000);
    }
    return newLine;
  }

  getObservable(key: TKey): Observable<TData> {
    return this.getLineOrCreate(key).getObservable();
  }

  getSyncStatusObservable(key: TKey): Observable<boolean> {
    return this.getLineOrCreate(key).getSyncStatusObservable();
  }

  getDataWithSyncStatusObservable(
    key: TKey
  ): Observable<WithSyncStatus<TData>> {
    return this.getLineOrCreate(key).getDataWithSyncStatusObservable();
  }

  getLine(key: TKey): DataLine<TData> | null {
    const keyString = this.keyToString(key);
    return this.subscriptionLineMap.get(keyString) ?? null;
  }

  getLineOrCreate(key: TKey): DataLine<TData> {
    const keyString = this.keyToString(key);
    return this.subscriptionLineMap.get(keyString) ?? this.createLine(key);
  }

  getLineOrCreateWithoutInitSync(key: TKey): DataLine<TData> {
    const keyString = this.keyToString(key);
    return (
      this.subscriptionLineMap.get(keyString) ?? this.createLine(key, true)
    );
  }

  optionalInitLineWithSyncAction(
    key: TKey,
    syncAction: (line: DataLine<TData>) => Promise<void>
  ): Promise<void> {
    const optionalLine = this.getLine(key);
    if (optionalLine != null) return Promise.resolve();
    const line = this.createLine(key, true);
    return line.syncWithAction(syncAction);
  }
}
