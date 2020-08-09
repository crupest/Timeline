import { pull } from 'lodash';
import { Observable } from 'rxjs';

export type Subscriber<TData> = (data: TData) => void;

export interface IDataLine<TData> {
  readonly value: undefined | TData;
  next(value: TData): void;
  readonly isSyncing: boolean;
  beginSync(): void;
  endSync(): void;
  endSyncAndNext(value: TData): void;
}

export class DataLine<TData> implements IDataLine<TData> {
  private _current: TData | undefined = undefined;

  private _syncing = false;

  private _observers: Subscriber<TData>[] = [];

  constructor(
    private config?: { destroyable?: (value: TData | undefined) => boolean }
  ) {}

  subscribe(subscriber: Subscriber<TData>): void {
    this._observers.push(subscriber);
    if (this._current !== undefined) {
      subscriber(this._current);
    }
  }

  unsubscribe(subscriber: Subscriber<TData>): void {
    if (!this._observers.includes(subscriber)) return;
    pull(this._observers, subscriber);
  }

  get value(): TData | undefined {
    return this._current;
  }

  next(value: TData): void {
    this._current = value;
    this._observers.forEach((observer) => observer(value));
  }

  get isSyncing(): boolean {
    return this._syncing;
  }

  beginSync(): void {
    if (!this._syncing) {
      this._syncing = true;
    }
  }

  endSync(): void {
    if (this._syncing) {
      this._syncing = false;
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
  private setup?: (key: TKey, line: IDataLine<TData>) => (() => void) | void;
  private destroyable?: (key: TKey, value: TData | undefined) => boolean;

  private readonly subscriptionLineMap = new Map<string, DataLine<TData>>();

  private cleanTimerId = 0;

  // setup is called after creating line and if it returns a function as destroyer, then when the line is destroyed the destroyer will be called.
  constructor(config?: {
    keyToString?: (key: TKey) => string;
    setup?: (key: TKey, line: IDataLine<TData>) => void;
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

  subscribe(key: TKey, subscriber: Subscriber<TData>): void {
    const keyString = this.keyToString(key);
    const line =
      this.subscriptionLineMap.get(keyString) ?? this.createLine(key);
    return line.subscribe(subscriber);
  }

  unsubscribe(key: TKey, subscriber: Subscriber<TData>): void {
    const keyString = this.keyToString(key);
    const line = this.subscriptionLineMap.get(keyString);
    return line?.unsubscribe(subscriber);
  }

  getObservable(key: TKey): Observable<TData> {
    return new Observable((observer) => {
      const f = (data: TData): void => {
        observer.next(data);
      };

      this.subscribe(key, f);
      return () => {
        this.unsubscribe(key, f);
      };
    });
  }

  getLine(key: TKey): IDataLine<TData> | null {
    const keyString = this.keyToString(key);
    return this.subscriptionLineMap.get(keyString) ?? null;
  }

  getLineOrCreateWithoutSetup(key: TKey): IDataLine<TData> {
    const keyString = this.keyToString(key);
    return (
      this.subscriptionLineMap.get(keyString) ?? this.createLine(key, false)
    );
  }
}
