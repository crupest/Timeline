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
const networkLatencyKey = 'mockServer.networkLatency';

let disableNetwork: boolean =
  localStorage.getItem(disableNetworkKey) === 'true' ? true : false;

const savedNetworkLatency = localStorage.getItem(networkLatencyKey);

let networkLatency: number | null =
  savedNetworkLatency != null ? Number(savedNetworkLatency) : null;

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

Object.defineProperty(window, 'networkLatency', {
  get: () => networkLatency,
  set: (value) => {
    if (typeof value === 'number') {
      networkLatency = value;
      localStorage.setItem(networkLatencyKey, value.toString());
    } else if (value == null) {
      networkLatency = null;
      localStorage.removeItem(networkLatencyKey);
    }
  },
});

export async function mockPrepare(key: string): Promise<void> {
  console.log(`Recieve request: ${key}`);

  if (disableNetwork) {
    console.warn('Network is disabled for mock server.');
    throw new HttpNetworkError();
  }
  if (networkLatency != null) {
    await new Promise((resolve) => {
      window.setTimeout(() => {
        resolve();
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
      }, networkLatency! * 1000);
    });
  }

  await Promise.resolve();
}
