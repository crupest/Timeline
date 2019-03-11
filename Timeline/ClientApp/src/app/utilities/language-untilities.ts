export function nullIfUndefined<T>(value: T | undefined): T | null {
  return value === undefined ? null : value;
}

export function throwIfNullOrUndefined<T>(value: T | null | undefined,
  lazyMessage: () => string = () => 'Value mustn\'t be falsy'): T | never {
  if (value === null || value === undefined) {
    throw new Error(lazyMessage());
  } else {
    return value;
  }
}
