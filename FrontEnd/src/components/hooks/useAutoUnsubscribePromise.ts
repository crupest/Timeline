import { useEffect, DependencyList } from "react";

export default function useAutoUnsubscribePromise<T>(
  promiseGenerator: () => Promise<T> | null | undefined,
  resultHandler: (data: T) => void,
  dependencies?: DependencyList | undefined,
) {
  useEffect(() => {
    let subscribe = true;
    const promise = promiseGenerator();
    if (promise) {
      void promise.then((data) => {
        if (subscribe) {
          resultHandler(data);
        }
      });

      return () => {
        subscribe = false;
      };
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [promiseGenerator, resultHandler, ...(dependencies ?? [])]);
}
