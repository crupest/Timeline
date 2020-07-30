// Remarks for SubscriptionHub:
// 1. Compared with 'push' sematics in rxjs subject, we need 'pull'. In other words, no subscription, no updating.
// 2. We need a way to finalize the last object. For example, if it has an object url, we need to revoke it.
// 3. Make api easier to use and write less boilerplate codes.
//
// Currently updator will wait for last update or creation to finish. So the old data passed to it will always be right. We may add feature for just cancel last one but not wait for it.
//
// There might be some bugs, especially memory leaks and in asynchronization codes.

import * as rxjs from 'rxjs';

export type Subscriber<TData> = (data: TData) => void;

export class Subscription {
  constructor(private _onUnsubscribe: () => void) {}

  unsubscribe(): void {
    this._onUnsubscribe();
  }
}

class SubscriptionToken {
  constructor(public _subscription: rxjs.Subscription) {}
}

class SubscriptionLine<TData> {
  private _lastDataPromise: Promise<TData>;
  private _dataSubject: rxjs.BehaviorSubject<TData>;
  private _refCount = 0;

  constructor(
    defaultValueProvider: () => TData,
    setup: ((old: TData) => Promise<TData>) | undefined,
    private _destroyer: ((data: TData) => void) | undefined,
    private _onZeroRef: (self: SubscriptionLine<TData>) => void
  ) {
    const initValue = defaultValueProvider();
    this._lastDataPromise = Promise.resolve(initValue);
    this._dataSubject = new rxjs.BehaviorSubject<TData>(initValue);
    if (setup != null) {
      this.next(setup);
    }
  }

  subscribe(subscriber: Subscriber<TData>): SubscriptionToken {
    const subscription = this._dataSubject.subscribe(subscriber);
    this._refCount += 1;
    return new SubscriptionToken(subscription);
  }

  unsubscribe(token: SubscriptionToken): void {
    token._subscription.unsubscribe();
    this._refCount -= 1;
    if (this._refCount === 0) {
      const { _destroyer: destroyer } = this;
      if (destroyer != null) {
        void this._lastDataPromise.then((data) => {
          destroyer(data);
        });
      }
      this._onZeroRef(this);
    }
  }

  next(updator: (old: TData) => Promise<TData>): void {
    this._lastDataPromise = this._lastDataPromise
      .then((old) => updator(old))
      .then((data) => {
        const last = this._dataSubject.value;
        if (this._destroyer != null) {
          this._destroyer(last);
        }
        this._dataSubject.next(data);
        return data;
      });
  }
}

export interface ISubscriptionHub<TKey, TData> {
  subscribe(key: TKey, subscriber: Subscriber<TData>): Subscription;
}

export class SubscriptionHub<TKey, TData>
  implements ISubscriptionHub<TKey, TData> {
  // If setup is set, update is called with setup immediately after setting default value.
  constructor(
    public keyToString: (key: TKey) => string,
    public defaultValueProvider: (key: TKey) => TData,
    public setup?: (key: TKey) => Promise<TData>,
    public destroyer?: (key: TKey, data: TData) => void
  ) {}

  private subscriptionLineMap = new Map<string, SubscriptionLine<TData>>();

  subscribe(key: TKey, subscriber: Subscriber<TData>): Subscription {
    const keyString = this.keyToString(key);
    const line = (() => {
      const savedLine = this.subscriptionLineMap.get(keyString);
      if (savedLine == null) {
        const { setup, destroyer } = this;
        const newLine = new SubscriptionLine<TData>(
          () => this.defaultValueProvider(key),
          setup != null ? () => setup(key) : undefined,
          destroyer != null
            ? (data) => {
                destroyer(key, data);
              }
            : undefined,
          () => {
            this.subscriptionLineMap.delete(keyString);
          }
        );
        this.subscriptionLineMap.set(keyString, newLine);
        return newLine;
      } else {
        return savedLine;
      }
    })();
    const token = line.subscribe(subscriber);
    return new Subscription(() => {
      line.unsubscribe(token);
    });
  }

  // Old data is destroyed automatically.
  // updator is called only if there is subscription.
  update(key: TKey, updator: (key: TKey, old: TData) => Promise<TData>): void {
    const keyString = this.keyToString(key);
    const line = this.subscriptionLineMap.get(keyString);
    if (line != null) {
      line.next((old) => updator(key, old));
    }
  }
}
