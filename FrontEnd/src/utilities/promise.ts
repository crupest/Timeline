export function subscribePromise<T>(
  promise: Promise<T>,
  resolve: Parameters<Promise<T>["then"]>[0],
  reject?: Parameters<Promise<T>["then"]>[1],
): {
  promise: ReturnType<Promise<T>["then"]>;
  (): void;
} {
  let subscribe = true;

  const p = promise.then(
    resolve != null
      ? (value) => {
          if (subscribe) {
            resolve(value);
          }
        }
      : undefined,
    reject != null
      ? (error) => {
          if (subscribe) {
            reject(error);
          }
        }
      : undefined,
  );

  const result = function () {
    subscribe = false;
  };
  result.promise = p;

  return result;
}

