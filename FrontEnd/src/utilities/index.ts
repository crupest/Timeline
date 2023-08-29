export { default as base64 } from "./base64";
export { withQuery } from "./url";

export function delay(milliseconds: number): Promise<void> {
  return new Promise<void>((resolve) => {
    setTimeout(() => {
      resolve();
    }, milliseconds);
  });
}
