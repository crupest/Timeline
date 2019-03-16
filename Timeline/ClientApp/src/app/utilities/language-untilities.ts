export function nullIfUndefined<T>(value: T | undefined): T | null {
  return value === undefined ? null : value;
}

export function throwIfNullOrUndefined<T>(value: T | null | undefined,
  message: string | (() => string) = 'Value mustn\'t be null or undefined'): T | never {
  if (value === null || value === undefined) {
    throw new Error(typeof message === 'string' ? message : message());
  } else {
    return value;
  }
}
