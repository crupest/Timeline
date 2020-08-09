import { pull } from 'lodash';
import { Observable, BehaviorSubject, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

export type Subscriber<TData> = (data: TData) => void;

export type WithSyncStatus<T> = T & { syncing: boolean };

export class DataLine<TData> {
  private _current: TData | undefined = undefined;

  private _syncingSubject = new BehaviorSubject<boolean>(false);

  private _observers: Subscriber<TData>[] = [];

  constructor(
    private config?: { destroyable?: (value: TData | undefined) => boolean }
  ) {}

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
    return this._syncingSubject.value;
  }

  beginSync(): void {
    if (!this._syncingSubject.value) {
      this._syncingSubject.next(true);
    }
  }

  endSync(): void {
    if (this._syncingSubject.value) {
      this._syncingSubject.next(false);
    }
  }

  get destroyable(): boolean {
    const customDestroyable = this.config?.destroyable;

    return (
      this._observers.length === 0 &&
      (customDestroyable != null ? customDestroyable(this._current) : true)
    );
  }

  endSyncAndNext(value: TData): void {
    this.endSync();
    this.next(value);
  }
}

export class DataHub<TKey, TData> {
  private keyToString: (key: TKey) => string;
  private setup?: (key: TKey, line: DataLine<TData>) => (() => void) | void;
  private destroyable?: (key: TKey, value: TData | undefined) => boolean;

  private readonly subscriptionLineMap = new Map<string, DataLine<TData>>();

  private cleanTimerId = 0;

  // setup is called after creating line and if it returns a function as destroyer, then when the line is destroyed the destroyer will be called.
  constructor(config?: {
    keyToString?: (key: TKey) => string;
    setup?: (key: TKey, line: DataLine<TData>) => void;
    destroyable?: (key: TKey, value: TData | undefined) => boolean;
  }) {
    this.keyToString =
      config?.keyToString ??
      ((value): string => {
        if (typeof value === 'string') return value;
        else
          throw new Error(
            'Default keyToString function only pass string value.'
          );
      });

    this.setup = config?.setup;
    this.destroyable = config?.destroyable;
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

  private createLine(key: TKey, useSetup = true): DataLine<TData> {
    const keyString = this.keyToString(key);
    const { setup, destroyable } = this;
    const newLine = new DataLine<TData>({
      destroyable:
        destroyable != null ? (value) => destroyable(key, value) : undefined,
    });
    this.subscriptionLineMap.set(keyString, newLine);
    if (useSetup) {
      setup?.(key, newLine);
    }
    if (this.subscriptionLineMap.size === 1) {
      this.cleanTimerId = window.setInterval(this.cleanLines.bind(this), 20000);
    }
    return newLine;
  }

  getObservable(key: TKey): Observable<TData> {
    return this.getLineOrCreateWithSetup(key).getObservable();
  }

  getSyncStatusObservable(key: TKey): Observable<boolean> {
    return this.getLineOrCreateWithSetup(key).getSyncStatusObservable();
  }

  getDataWithSyncStatusObservable(
    key: TKey
  ): Observable<WithSyncStatus<TData>> {
    return this.getLineOrCreateWithSetup(key).getDataWithSyncStatusObservable();
  }

  getLine(key: TKey): DataLine<TData> | null {
    const keyString = this.keyToString(key);
    return this.subscriptionLineMap.get(keyString) ?? null;
  }

  getLineOrCreateWithSetup(key: TKey): DataLine<TData> {
    const keyString = this.keyToString(key);
    return this.subscriptionLineMap.get(keyString) ?? this.createLine(key);
  }

  getLineOrCreateWithoutSetup(key: TKey): DataLine<TData> {
    const keyString = this.keyToString(key);
    return (
      this.subscriptionLineMap.get(keyString) ?? this.createLine(key, false)
    );
  }
}
