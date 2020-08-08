import React, { useState, useEffect } from 'react';
import { BehaviorSubject, Observable, from } from 'rxjs';

import { UiLogicError } from '../common';
import { convertError } from '../utilities/rxjs';
import { pushAlert } from '../common/alert-service';

import { dataStorage } from './common';
import { SubscriptionHub, ISubscriptionHub } from './SubscriptionHub';

import { HttpNetworkError, BlobWithEtag, NotModified } from '../http/common';
import {
  getHttpTokenClient,
  HttpCreateTokenBadCredentialError,
} from '../http/token';
import {
  getHttpUserClient,
  HttpUserNotExistError,
  HttpUser,
} from '../http/user';

export type User = HttpUser;

export interface UserAuthInfo {
  username: string;
  administrator: boolean;
}

export interface UserWithToken extends User {
  token: string;
}

export interface LoginCredentials {
  username: string;
  password: string;
}

export class BadCredentialError {
  message = 'login.badCredential';
}

const USER_STORAGE_KEY = 'currentuser';

export class UserService {
  private userSubject = new BehaviorSubject<UserWithToken | null | undefined>(
    undefined
  );

  get user$(): Observable<UserWithToken | null | undefined> {
    return this.userSubject;
  }

  get currentUser(): UserWithToken | null | undefined {
    return this.userSubject.value;
  }

  async checkLoginState(): Promise<UserWithToken | null> {
    if (this.currentUser !== undefined) {
      console.warn("Already checked user. Can't check twice.");
    }

    const savedUser = await dataStorage.getItem<UserWithToken | null>(
      USER_STORAGE_KEY
    );

    if (savedUser == null) {
      this.userSubject.next(null);
      return null;
    }

    this.userSubject.next(savedUser);

    const savedToken = savedUser.token;
    try {
      const res = await getHttpTokenClient().verify({ token: savedToken });
      const user: UserWithToken = { ...res.user, token: savedToken };
      await dataStorage.setItem<UserWithToken>(USER_STORAGE_KEY, user);
      this.userSubject.next(user);
      pushAlert({
        type: 'success',
        message: {
          type: 'i18n',
          key: 'user.welcomeBack',
        },
      });
      return user;
    } catch (error) {
      if (error instanceof HttpNetworkError) {
        pushAlert({
          type: 'danger',
          message: { type: 'i18n', key: 'user.verifyTokenFailedNetwork' },
        });
        return savedUser;
      } else {
        await dataStorage.removeItem(USER_STORAGE_KEY);
        this.userSubject.next(null);
        pushAlert({
          type: 'danger',
          message: { type: 'i18n', key: 'user.verifyTokenFailed' },
        });
        return null;
      }
    }
  }

  async login(
    credentials: LoginCredentials,
    rememberMe: boolean
  ): Promise<void> {
    if (this.currentUser) {
      throw new UiLogicError('Already login.');
    }
    try {
      const res = await getHttpTokenClient().create({
        ...credentials,
        expire: 30,
      });
      const user: UserWithToken = {
        ...res.user,
        token: res.token,
      };
      if (rememberMe) {
        await dataStorage.setItem<UserWithToken>(USER_STORAGE_KEY, user);
      }
      this.userSubject.next(user);
    } catch (e) {
      if (e instanceof HttpCreateTokenBadCredentialError) {
        throw new BadCredentialError();
      } else {
        throw e;
      }
    }
  }

  async logout(): Promise<void> {
    if (this.currentUser === undefined) {
      throw new UiLogicError('Please check user first.');
    }
    if (this.currentUser === null) {
      throw new UiLogicError('No login.');
    }
    await dataStorage.removeItem(USER_STORAGE_KEY);
    this.userSubject.next(null);
  }

  changePassword(
    oldPassword: string,
    newPassword: string
  ): Observable<unknown> {
    if (this.currentUser == undefined) {
      throw new UiLogicError("Not login or checked now, can't log out.");
    }
    const $ = from(
      getHttpUserClient().changePassword(
        {
          oldPassword,
          newPassword,
        },
        this.currentUser.token
      )
    );
    $.subscribe(() => {
      void this.logout();
    });
    return $;
  }
}

export const userService = new UserService();

export function useRawUser(): UserWithToken | null | undefined {
  const [user, setUser] = useState<UserWithToken | null | undefined>(
    userService.currentUser
  );
  useEffect(() => {
    const subscription = userService.user$.subscribe((u) => setUser(u));
    return () => {
      subscription.unsubscribe();
    };
  });
  return user;
}

export function useUser(): UserWithToken | null {
  const [user, setUser] = useState<UserWithToken | null>(() => {
    const initUser = userService.currentUser;
    if (initUser === undefined) {
      throw new UiLogicError(
        "This is a logic error in user module. Current user can't be undefined in useUser."
      );
    }
    return initUser;
  });
  useEffect(() => {
    const sub = userService.user$.subscribe((u) => {
      if (u === undefined) {
        throw new UiLogicError(
          "This is a logic error in user module. User emitted can't be undefined later."
        );
      }
      setUser(u);
    });
    return () => {
      sub.unsubscribe();
    };
  });
  return user;
}

export function useUserLoggedIn(): UserWithToken {
  const user = useUser();
  if (user == null) {
    throw new UiLogicError('You assert user has logged in but actually not.');
  }
  return user;
}

export function checkLogin(): UserWithToken {
  const user = userService.currentUser;
  if (user == null) {
    throw new UiLogicError('You must login to perform the operation.');
  }
  return user;
}

export class UserNotExistError extends Error {}

export class UserInfoService {
  private getAvatarKey(username: string): string {
    return `user.${username}.avatar`;
  }

  private getCachedAvatar(username: string): Promise<Blob | null> {
    return dataStorage
      .getItem<BlobWithEtag | null>(this.getAvatarKey(username))
      .then((data) => data?.data ?? null);
  }

  private async fetchAndCacheAvatar(
    username: string
  ): Promise<{ data: Blob; type: 'synced' | 'cache' } | 'offline'> {
    const key = this.getAvatarKey(username);
    const cache = await dataStorage.getItem<BlobWithEtag | null>(key);
    if (cache == null) {
      try {
        const avatar = await getHttpUserClient().getAvatar(username);
        await dataStorage.setItem<BlobWithEtag>(key, avatar);
        return {
          data: avatar.data,
          type: 'synced',
        };
      } catch (e) {
        if (e instanceof HttpNetworkError) {
          return 'offline';
        } else {
          throw e;
        }
      }
    } else {
      try {
        const res = await getHttpUserClient().getAvatar(username, cache.etag);
        if (res instanceof NotModified) {
          return {
            data: cache.data,
            type: 'synced',
          };
        } else {
          const avatar = res;
          await dataStorage.setItem<BlobWithEtag>(key, avatar);
          return {
            data: avatar.data,
            type: 'synced',
          };
        }
      } catch (e) {
        if (e instanceof HttpNetworkError) {
          return {
            data: cache.data,
            type: 'cache',
          };
        } else {
          throw e;
        }
      }
    }
  }

  private _avatarSubscriptionHub = new SubscriptionHub<string, Blob>({
    setup: (key, line) => {
      void this.getCachedAvatar(key)
        .then((avatar) => {
          if (avatar != null) {
            line.next(avatar);
          }
        })
        .then(() => {
          return this.fetchAndCacheAvatar(key);
        })
        .then((result) => {
          if (result !== 'offline') {
            line.next(result.data);
          }
        });
    },
  });

  getUserInfo(username: string): Observable<User> {
    return from(getHttpUserClient().get(username)).pipe(
      convertError(HttpUserNotExistError, UserNotExistError)
    );
  }

  async setAvatar(username: string, blob: Blob): Promise<void> {
    const user = checkLogin();
    await getHttpUserClient().putAvatar(username, blob, user.token);
    this._avatarSubscriptionHub.getLine(username)?.next(blob);
  }

  get avatarHub(): ISubscriptionHub<string, Blob> {
    return this._avatarSubscriptionHub;
  }
}

export const userInfoService = new UserInfoService();

export function useAvatar(username?: string): Blob | undefined {
  const [state, setState] = React.useState<Blob | undefined>(undefined);
  React.useEffect(() => {
    if (username == null) {
      setState(undefined);
      return;
    }

    const subscription = userInfoService.avatarHub.subscribe(
      username,
      (blob) => {
        setState(blob);
      }
    );
    return () => {
      subscription.unsubscribe();
    };
  }, [username]);
  return state;
}
