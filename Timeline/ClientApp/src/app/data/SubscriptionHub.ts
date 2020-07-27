// Remarks for SubscriptionHub:
// 1. Compared with 'push' sematics in rxjs subject, we need 'pull'. In other words, no subscription, no updating.
// 2. We need a way to finalize the last object. For example, if it has an object url, we need to revoke it.
// 3. Make api easier to use and write less boilerplate codes.
//
// Currently updator will wait for last update or creation to finish. So the old data passed to it will always be right. We may add feature for just cancel last one but not wait for it.
//
// There might be some bugs, especially memory leaks and in asynchronization codes.

import * as rxjs from 'rxjs';
import { filter } from 'rxjs/operators';

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
  private _dataSubject = new rxjs.BehaviorSubject<TData | undefined>(undefined);
  private _data$: rxjs.Observable<TData> = this._dataSubject.pipe(
    filter((d) => d !== undefined)
  ) as rxjs.Observable<TData>;
  private _refCount = 0;

  constructor(
    _creator: () => Promise<TData>,
    private _destroyer: ((data: TData) => void) | undefined,
    private _onZeroRef: (self: SubscriptionLine<TData>) => void
  ) {
    this._lastDataPromise = _creator().then((data) => {
      this._dataSubject.next(data);
      return data;
    });
  }

  subscribe(subscriber: Subscriber<TData>): SubscriptionToken {
    const subscription = this._data$.subscribe(subscriber);
    this._refCount += 1;
    return new SubscriptionToken(subscription);
  }

  unsubscribe(token: SubscriptionToken): void {
    token._subscription.unsubscribe();
    this._refCount -= 1;
    if (this._refCount === 0) {
      void this._lastDataPromise.then((data) => {
        if (this._destroyer != null && data !== undefined) {
          this._destroyer(data);
        }
      });
      this._onZeroRef(this);
    }
  }

  next(updator: (old: TData) => Promise<TData>): void {
    this._lastDataPromise = this._lastDataPromise
      .then((old) => updator(old))
      .then((data) => {
        const last = this._dataSubject.value;
        if (this._destroyer != null && last !== undefined) {
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
  constructor(
    public keyToString: (key: TKey) => string,
    public creator: (key: TKey) => Promise<TData>,
    public destroyer?: (key: TKey, data: TData) => void
  ) {}

  private subscriptionLineMap = new Map<string, SubscriptionLine<TData>>();

  subscribe(key: TKey, subscriber: Subscriber<TData>): Subscription {
    const keyString = this.keyToString(key);
    const line = (() => {
      const savedLine = this.subscriptionLineMap.get(keyString);
      if (savedLine == null) {
        const { destroyer } = this;
        const newLine = new SubscriptionLine<TData>(
          () => this.creator(key),
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
  update(key: TKey, updator: (key: TKey) => Promise<TData>): void {
    const keyString = this.keyToString(key);
    const line = this.subscriptionLineMap.get(keyString);
    if (line != null) {
      line.next(() => updator(key));
    }
  }
}
