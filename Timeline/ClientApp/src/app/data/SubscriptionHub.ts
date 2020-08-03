// Remarks for SubscriptionHub:
// 1. Compared with 'push' sematics in rxjs subject, we need 'pull'. In other words, no subscription, no updating.
// 2. Make api easier to use and write less boilerplate codes.
//
// There might be some bugs, especially memory leaks and in asynchronization codes.

import { pull } from 'lodash';

export type Subscriber<TData> = (data: TData) => void;

export class Subscription {
  constructor(private _onUnsubscribe: () => void) {}

  unsubscribe(): void {
    this._onUnsubscribe();
  }
}

class NoValue {}

export class SubscriptionLine<TData> {
  private _current: TData | NoValue = new NoValue();

  private _observers: Subscriber<TData>[] = [];

  constructor(private config?: { onZeroObserver?: () => void }) {}

  subscribe(subscriber: Subscriber<TData>): Subscription {
    this._observers.push(subscriber);
    if (!(this._current instanceof NoValue)) {
      subscriber(this._current);
    }

    return new Subscription(() => this.unsubscribe(subscriber));
  }

  private unsubscribe(subscriber: Subscriber<TData>): void {
    if (!this._observers.includes(subscriber)) return;
    pull(this._observers, subscriber);
    if (this._observers.length === 0) {
      this?.config?.onZeroObserver?.();
    }
  }

  next(value: TData): void {
    this._observers.forEach((observer) => observer(value));
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
    next: (value: TData) => void
  ) => (() => void) | void;

  private readonly subscriptionLineMap = new Map<
    string,
    {
      line: SubscriptionLine<TData>;
      destroyer: (() => void) | undefined;
      destroyTimer?: number; // Cancel it when resubscribe.
    }
  >();

  // setup is called after creating line and if it returns a function as destroyer, then when the line is destroyed the destroyer will be called.
  constructor(config?: {
    keyToString?: (key: TKey) => string;
    setup?: (key: TKey, next: (value: TData) => void) => (() => void) | void;
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
  }

  subscribe(key: TKey, subscriber: Subscriber<TData>): Subscription {
    const keyString = this.keyToString(key);
    const line = (() => {
      const info = this.subscriptionLineMap.get(keyString);
      if (info == null) {
        const { setup } = this;
        const newLine = new SubscriptionLine<TData>({
          onZeroObserver: () => {
            const i = this.subscriptionLineMap.get(keyString);
            if (i != null) {
              i.destroyTimer = window.setTimeout(() => {
                i.destroyer?.();
                this.subscriptionLineMap.delete(keyString);
              }, 10000);
            }
          },
        });
        const destroyer = setup?.(key, newLine.next.bind(newLine));
        this.subscriptionLineMap.set(keyString, {
          line: newLine,
          destroyer: destroyer != null ? destroyer : undefined,
        });
        return newLine;
      } else {
        if (info.destroyTimer != null) {
          window.clearTimeout(info.destroyTimer);
          info.destroyTimer = undefined;
        }
        return info.line;
      }
    })();
    return line.subscribe(subscriber);
  }

  update(key: TKey, value: TData): void {
    const keyString = this.keyToString(key);
    const info = this.subscriptionLineMap.get(keyString);
    if (info != null) {
      info.line.next(value);
    }
  }
}
