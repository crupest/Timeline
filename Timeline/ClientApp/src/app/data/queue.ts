const queueMap = new Map<string, Promise<null>>();

export function queue<T>(key: string, func: () => Promise<T>): Promise<T> {
  const last = queueMap.get(key);
  if (last == null) {
    const promise = func();
    queueMap.set(key, promise.then(null, null));
    return promise;
  } else {
    const promise = last.then(() => func());
    queueMap.set(key, promise.then(null, null));
    return promise;
  }
}
