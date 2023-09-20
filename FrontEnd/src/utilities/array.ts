export function copy_move<T>(
  array: T[],
  oldIndex: number,
  newIndex: number,
): T[] {
  if (oldIndex < 0 || oldIndex >= array.length) {
    throw new Error("Old index out of range.");
  }

  if (newIndex < 0) {
    newIndex = 0;
  }

  if (newIndex >= array.length) {
    newIndex = array.length - 1;
  }

  const result = array.slice();
  const [element] = result.splice(oldIndex, 1);
  result.splice(newIndex, 0, element);

  return result;
}

export function copy_insert<T>(array: T[], index: number, element: T): T[] {
  const result = array.slice();
  result.splice(index, 0, element);
  return result;
}

export function copy_push<T>(array: T[], element: T): T[] {
  const result = array.slice();
  result.push(element);
  return result;
}

export function copy_delete<T>(array: T[], index: number): T[] {
  const result = array.slice();
  result.splice(index, 1);
  return array;
}
