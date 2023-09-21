export { default as base64 } from "./base64";
export { withQuery } from "./url";
export { subscribePromise } from "./promise";

export function delay(milliseconds: number): Promise<void> {
  return new Promise<void>((resolve) => {
    setTimeout(() => {
      resolve();
    }, milliseconds);
  });
}

export function range(stop: number): number[];
export function range(start: number, stop: number, step?: number): number[];
export function range(start: number, stop?: number, step?: number): number[] {
  if (stop == undefined) {
    stop = start;
    start = 0;
  }
  if (step == undefined) {
    step = 1;
  }
  const result: number[] = [];
  for (let i = start; i < stop; i += step) {
    result.push(i);
  }
  return result;
}
