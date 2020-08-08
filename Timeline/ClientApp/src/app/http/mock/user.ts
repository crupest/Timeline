import axios from 'axios';

import { BlobWithEtag, NotModified } from '../common';
import {
  IHttpUserClient,
  HttpUser,
  HttpUserNotExistError,
  HttpUserPatchRequest,
  HttpChangePasswordRequest,
} from '../user';

import { mockStorage, sha1, mockPrepare } from './common';

import defaultAvatarUrl from './default-avatar.png';

let _defaultAvatar: BlobWithEtag | undefined = undefined;

async function getDefaultAvatar(): Promise<BlobWithEtag> {
  if (_defaultAvatar == null) {
    const blob = (
      await axios.get<Blob>(defaultAvatarUrl, {
        responseType: 'blob',
      })
    ).data;
    const etag = await sha1(blob);
    _defaultAvatar = {
      data: blob,
      etag,
    };
  }
  return _defaultAvatar;
}

export class MockTokenError extends Error {
  constructor() {
    super('Token bad format.');
  }
}

export class MockUserNotExistError extends Error {
  constructor() {
    super('Only two user "user" and "admin".');
  }
}

export function checkUsername(
  username: string
): asserts username is 'user' | 'admin' {
  if (!['user', 'admin'].includes(username)) throw new MockUserNotExistError();
}

export function checkToken(token: string): string {
  if (!token.startsWith('token-')) {
    throw new MockTokenError();
  }
  return token.substr(6);
}

const uniqueIdMap = {
  user: 'e4c80127d092d9b2fc19c5e04612d4c0',
  admin: '5640fa45435f9a55077b9f77c42a77bb',
};

export async function getUser(
  username: 'user' | 'admin' | string
): Promise<HttpUser> {
  checkUsername(username);
  const savedNickname = await mockStorage.getItem<string>(
    `user.${username}.nickname`
  );
  return {
    uniqueId: uniqueIdMap[username],
    username: username,
    nickname:
      savedNickname == null || savedNickname === '' ? username : savedNickname,
    administrator: username === 'admin',
  };
}

export class MockHttpUserClient implements IHttpUserClient {
  async get(username: string): Promise<HttpUser> {
    await mockPrepare('user.get');
    return await getUser(username).catch((e) => {
      if (e instanceof MockUserNotExistError) {
        throw new HttpUserNotExistError();
      } else {
        throw e;
      }
    });
  }

  async patch(
    username: string,
    req: HttpUserPatchRequest,
    _token: string
  ): Promise<HttpUser> {
    await mockPrepare('user.patch');
    if (req.nickname != null) {
      await mockStorage.setItem(`user.${username}.nickname`, req.nickname);
    }
    return await getUser(username);
  }

  getAvatar(username: string): Promise<BlobWithEtag>;
  async getAvatar(
    username: string,
    etag?: string
  ): Promise<BlobWithEtag | NotModified> {
    await mockPrepare('user.avatar.get');

    const savedEtag = await mockStorage.getItem(`user.${username}.avatar.etag`);
    if (savedEtag == null) {
      return await getDefaultAvatar();
    }

    if (savedEtag === etag) {
      return new NotModified();
    }

    return {
      data: await mockStorage.getItem<Blob>(`user.${username}.avatar.data`),
      etag: await mockStorage.getItem<string>(`user.${username}.avatar.etag`),
    };
  }

  async putAvatar(username: string, data: Blob, _token: string): Promise<void> {
    await mockPrepare('user.avatar.put');
    const etag = await sha1(data);
    await mockStorage.setItem<Blob>(`user.${username}.avatar.data`, data);
    await mockStorage.setItem<string>(`user.${username}.avatar.etag`, etag);
  }

  async changePassword(
    _req: HttpChangePasswordRequest,
    _token: string
  ): Promise<void> {
    await mockPrepare('userop.changepassowrd');
    throw new Error('Not Implemented.');
  }
}
