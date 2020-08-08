import { pull } from 'lodash';
import { Observable } from 'rxjs';

export type Subscriber<TData> = (data: TData) => void;

export class Subscription {
  constructor(private _onUnsubscribe: () => void) {}

  unsubscribe(): void {
    this._onUnsubscribe();
  }
}

export interface ISubscriptionLine<TData> {
  readonly value: undefined | TData;
  next(value: TData): void;
}

export class SubscriptionLine<TData> implements ISubscriptionLine<TData> {
  private _current: TData | undefined = undefined;

  private _observers: Subscriber<TData>[] = [];

  constructor(
    private config?: { destroyable?: (value: TData | undefined) => boolean }
  ) {}

  subscribe(subscriber: Subscriber<TData>): Subscription {
    this._observers.push(subscriber);
    if (this._current !== undefined) {
      subscriber(this._current);
    }
    return new Subscription(() => this.unsubscribe(subscriber));
  }

  private unsubscribe(subscriber: Subscriber<TData>): void {
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

  get destroyable(): boolean {
    const customDestroyable = this.config?.destroyable;

    return (
      this._observers.length === 0 &&
      (customDestroyable != null ? customDestroyable(this._current) : true)
    );
  }
}

export interface ISubscriptionHub<TKey, TData> {
  subscribe(key: TKey, subscriber: Subscriber<TData>): Subscription;
}

export class SubscriptionHub<TKey, TData>
  implements ISubscriptionHub<TKey, TData> {
  private keyToString: (key: TKey) => string;
  private setup?: (
    key: TKey,
    line: ISubscriptionLine<TData>
  ) => (() => void) | void;
  private destroyable?: (key: TKey, value: TData | undefined) => boolean;

  private readonly subscriptionLineMap = new Map<
    string,
    {
      line: SubscriptionLine<TData>;
      destroyer: (() => void) | undefined;
    }
  >();

  private cleanTimerId = 0;

  // setup is called after creating line and if it returns a function as destroyer, then when the line is destroyed the destroyer will be called.
  constructor(config?: {
    keyToString?: (key: TKey) => string;
    setup?: (key: TKey, line: ISubscriptionLine<TData>) => (() => void) | void;
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
    for (const [key, info] of this.subscriptionLineMap.entries()) {
      if (info.line.destroyable) {
        info.destroyer?.();
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

  subscribe(key: TKey, subscriber: Subscriber<TData>): Subscription {
    const keyString = this.keyToString(key);
    const line = (() => {
      const info = this.subscriptionLineMap.get(keyString);
      if (info == null) {
        const { setup, destroyable } = this;
        const newLine = new SubscriptionLine<TData>({
          destroyable:
            destroyable != null
              ? (value) => destroyable(key, value)
              : undefined,
        });
        this.subscriptionLineMap.set(keyString, {
          line: newLine,
          destroyer: undefined,
        });
        const destroyer = setup?.(key, newLine);
        if (this.subscriptionLineMap.size === 0) {
          this.cleanTimerId = window.setInterval(
            this.cleanLines.bind(this),
            20000
          );
        }
        this.subscriptionLineMap.set(keyString, {
          line: newLine,
          destroyer: destroyer != null ? destroyer : undefined,
        });
        return newLine;
      } else {
        return info.line;
      }
    })();
    return line.subscribe(subscriber);
  }

  getObservable(key: TKey): Observable<TData> {
    return new Observable((observer) => {
      const sub = this.subscribe(key, (data) => {
        observer.next(data);
      });
      return () => {
        sub.unsubscribe();
      };
    });
  }

  getLine(key: TKey): ISubscriptionLine<TData> | null {
    const keyString = this.keyToString(key);
    return this.subscriptionLineMap.get(keyString)?.line ?? null;
  }
}
