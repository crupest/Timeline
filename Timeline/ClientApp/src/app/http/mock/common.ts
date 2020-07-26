import localforage from 'localforage';
import { SHA1 } from 'crypto-js';

import { HttpNetworkError } from '../common';

export const mockStorage = localforage.createInstance({
  name: 'mock-backend',
  description: 'Database for mock back end.',
  driver: localforage.INDEXEDDB,
});

export async function sha1(data: Blob): Promise<string> {
  const s = await new Promise<string>((resolve) => {
    const fileReader = new FileReader();
    fileReader.readAsBinaryString(data);
    fileReader.onload = () => {
      resolve(fileReader.result as string);
    };
  });

  return SHA1(s).toString();
}

const disableNetworkKey = 'mockServer.disableNetwork';

let disableNetwork: boolean =
  localStorage.getItem(disableNetworkKey) === 'true' ? true : false;

Object.defineProperty(window, 'disableNetwork', {
  get: () => disableNetwork,
  set: (value) => {
    if (value) {
      disableNetwork = true;
      localStorage.setItem(disableNetworkKey, 'true');
    } else {
      disableNetwork = false;
      localStorage.setItem(disableNetworkKey, 'false');
    }
  },
});

export async function mockPrepare(): Promise<void> {
  if (disableNetwork) {
    console.warn('Network is disabled for mock server.');
    throw new HttpNetworkError();
  }
  await Promise.resolve();
}
