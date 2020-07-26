import React, { useState, useEffect } from 'react';
import { BehaviorSubject, Observable, of, from } from 'rxjs';
import { map } from 'rxjs/operators';

import { UiLogicError } from '../common';
import { convertError } from '../utilities/rxjs';
import { pushAlert } from '../common/alert-service';

import { SubscriptionHub, ISubscriptionHub } from './SubscriptionHub';

import { HttpNetworkError } from '../http/common';
import {
  getHttpTokenClient,
  HttpCreateTokenBadCredentialError,
} from '../http/token';
import {
  getHttpUserClient,
  HttpUserNotExistError,
  HttpUser,
} from '../http/user';

import { BlobWithUrl } from './common';

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

const TOKEN_STORAGE_KEY = 'token';

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

  checkLoginState(): Observable<UserWithToken | null> {
    if (this.currentUser !== undefined)
      throw new UiLogicError("Already checked user. Can't check twice.");

    const savedToken = window.localStorage.getItem(TOKEN_STORAGE_KEY);
    if (savedToken) {
      const u$ = from(getHttpTokenClient().verify({ token: savedToken })).pipe(
        map(
          (res) =>
            ({
              ...res.user,
              token: savedToken,
            } as UserWithToken)
        )
      );
      u$.subscribe(
        (user) => {
          if (user != null) {
            pushAlert({
              type: 'success',
              message: {
                type: 'i18n',
                key: 'user.welcomeBack',
              },
            });
          }
          this.userSubject.next(user);
        },
        (error) => {
          if (error instanceof HttpNetworkError) {
            pushAlert({
              type: 'danger',
              message: { type: 'i18n', key: 'user.verifyTokenFailedNetwork' },
            });
          } else {
            window.localStorage.removeItem(TOKEN_STORAGE_KEY);
            pushAlert({
              type: 'danger',
              message: { type: 'i18n', key: 'user.verifyTokenFailed' },
            });
          }
          this.userSubject.next(null);
        }
      );
      return u$;
    }
    this.userSubject.next(null);
    return of(null);
  }

  login(
    credentials: LoginCredentials,
    rememberMe: boolean
  ): Observable<UserWithToken> {
    if (this.currentUser) {
      throw new UiLogicError('Already login.');
    }
    const u$ = from(
      getHttpTokenClient().create({
        ...credentials,
        expire: 30,
      })
    ).pipe(
      map(
        (res) =>
          ({
            ...res.user,
            token: res.token,
          } as UserWithToken)
      ),
      convertError(HttpCreateTokenBadCredentialError, BadCredentialError)
    );
    u$.subscribe((user) => {
      if (rememberMe) {
        window.localStorage.setItem(TOKEN_STORAGE_KEY, user.token);
      }
      this.userSubject.next(user);
    });
    return u$;
  }

  logout(): void {
    if (this.currentUser === undefined) {
      throw new UiLogicError('Please check user first.');
    }
    if (this.currentUser === null) {
      throw new UiLogicError('No login.');
    }
    window.localStorage.removeItem(TOKEN_STORAGE_KEY);
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
      this.logout();
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

export type AvatarInfo = BlobWithUrl;

export class UserInfoService {
  private _avatarSubscriptionHub = new SubscriptionHub<string, AvatarInfo>(
    (key) => key,
    async (key) => {
      const blob = (await getHttpUserClient().getAvatar(key)).data;
      const url = URL.createObjectURL(blob);
      return {
        blob,
        url,
      };
    },
    (_key, data) => {
      URL.revokeObjectURL(data.url);
    }
  );

  getUserInfo(username: string): Observable<User> {
    return from(getHttpUserClient().get(username)).pipe(
      convertError(HttpUserNotExistError, UserNotExistError)
    );
  }

  async setAvatar(username: string, blob: Blob): Promise<void> {
    const user = checkLogin();
    await getHttpUserClient().putAvatar(username, blob, user.token);
    this._avatarSubscriptionHub.update(username, () =>
      Promise.resolve({
        blob,
        url: URL.createObjectURL(blob),
      })
    );
  }

  get avatarHub(): ISubscriptionHub<string, AvatarInfo> {
    return this._avatarSubscriptionHub;
  }
}

export const userInfoService = new UserInfoService();

export function useAvatarUrl(username?: string): string | undefined {
  const [avatarUrl, setAvatarUrl] = React.useState<string | undefined>(
    undefined
  );
  React.useEffect(() => {
    if (username == null) {
      setAvatarUrl(undefined);
      return;
    }

    const subscription = userInfoService.avatarHub.subscribe(
      username,
      ({ url }) => {
        setAvatarUrl(url);
      }
    );
    return () => {
      subscription.unsubscribe();
    };
  }, [username]);
  return avatarUrl;
}
