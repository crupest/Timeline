import { Mock } from './mock';
import { nullIfUndefined } from '../utilities/language-untilities';

export function createMockStorage(): Mock<Storage> {
  const map: { [key: string]: string } = {};
  return {
    get length(): number {
      return Object.keys(map).length;
    },
    key(index: number): string | null {
      const keys = Object.keys(map);
      if (index >= keys.length) { return null; }
      return keys[index];
    },
    clear() {
      Object.keys(map).forEach(key => delete map.key);
    },
    getItem(key: string): string | null {
      return nullIfUndefined(map[key]);
    },
    setItem(key: string, value: string) {
      map[key] = value;
    },
    removeItem(key: string) {
      delete map[key];
    }
  };
}
