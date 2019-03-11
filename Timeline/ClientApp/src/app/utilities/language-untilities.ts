export function nullIfUndefined<T>(value: T | undefined): T | null {
  return value === undefined ? null : value;
}

export function throwIfFalsy(value: any, name: string = '<unknown name>') {
  if (!value) {
    throw new Error(name + ' is falsy.');
  }
}
