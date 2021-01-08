import React, { useState, useEffect } from "react";
import { BehaviorSubject, Observable, from } from "rxjs";
import { map, filter } from "rxjs/operators";

import { UiLogicError } from "@/common";
import { convertError } from "@/utilities/rxjs";

import {
  HttpNetworkError,
  BlobWithEtag,
  NotModified,
  tokenSubject,
} from "@/http/common";
import {
  getHttpTokenClient,
  HttpCreateTokenBadCredentialError,
} from "@/http/token";
import {
  getHttpUserClient,
  HttpUserNotExistError,
  HttpUser,
  UserPermission,
} from "@/http/user";

import { dataStorage, throwIfNotNetworkError } from "./common";
import { DataHub } from "./DataHub";
import { pushAlert } from "./alert";

export type User = HttpUser;

export class AuthUser implements User {
  constructor(user: User, public token: string) {
    this.uniqueId = user.uniqueId;
    this.username = user.username;
    this.permissions = user.permissions;
    this.nickname = user.nickname;
  }

  uniqueId: string;
  username: string;
  permissions: UserPermission[];
  nickname: string;

  get hasAdministrationPermission(): boolean {
    return this.permissions.length !== 0;
  }

  get hasAllTimelineAdministrationPermission(): boolean {
    return this.permissions.includes("AllTimelineManagement");
  }

  get hasHighlightTimelineAdministrationPermission(): boolean {
    return this.permissions.includes("HighlightTimelineManagement");
  }
}

export interface LoginCredentials {
  username: string;
  password: string;
}

export class BadCredentialError {
  message = "login.badCredential";
}

const USER_STORAGE_KEY = "currentuser";

export class UserService {
  constructor() {
    this.userSubject.subscribe((u) => {
      tokenSubject.next(u?.token ?? null);
    });
  }

  private userSubject = new BehaviorSubject<AuthUser | null | undefined>(
    undefined
  );

  get user$(): Observable<AuthUser | null | undefined> {
    return this.userSubject;
  }

  get currentUser(): AuthUser | null | undefined {
    return this.userSubject.value;
  }

  async checkLoginState(): Promise<AuthUser | null> {
    if (this.currentUser !== undefined) {
      console.warn("Already checked user. Can't check twice.");
    }

    const savedUser = await dataStorage.getItem<AuthUser | null>(
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
      const user = new AuthUser(res.user, savedToken);
      await dataStorage.setItem<AuthUser>(USER_STORAGE_KEY, user);
      this.userSubject.next(user);
      pushAlert({
        type: "success",
        message: {
          type: "i18n",
          key: "user.welcomeBack",
        },
      });
      return user;
    } catch (error) {
      if (error instanceof HttpNetworkError) {
        pushAlert({
          type: "danger",
          message: { type: "i18n", key: "user.verifyTokenFailedNetwork" },
        });
        return savedUser;
      } else {
        await dataStorage.removeItem(USER_STORAGE_KEY);
        this.userSubject.next(null);
        pushAlert({
          type: "danger",
          message: { type: "i18n", key: "user.verifyTokenFailed" },
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
      throw new UiLogicError("Already login.");
    }
    try {
      const res = await getHttpTokenClient().create({
        ...credentials,
        expire: 30,
      });
      const user = new AuthUser(res.user, res.token);
      if (rememberMe) {
        await dataStorage.setItem<AuthUser>(USER_STORAGE_KEY, user);
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
      throw new UiLogicError("Please check user first.");
    }
    if (this.currentUser === null) {
      throw new UiLogicError("No login.");
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
      getHttpUserClient().changePassword({
        oldPassword,
        newPassword,
      })
    );
    $.subscribe(() => {
      void this.logout();
    });
    return $;
  }
}

export const userService = new UserService();

export function useRawUser(): AuthUser | null | undefined {
  const [user, setUser] = useState<AuthUser | null | undefined>(
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

export function useUser(): AuthUser | null {
  const [user, setUser] = useState<AuthUser | null>(() => {
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

export function useUserLoggedIn(): AuthUser {
  const user = useUser();
  if (user == null) {
    throw new UiLogicError("You assert user has logged in but actually not.");
  }
  return user;
}

export function checkLogin(): AuthUser {
  const user = userService.currentUser;
  if (user == null) {
    throw new UiLogicError("You must login to perform the operation.");
  }
  return user;
}

export class UserNotExistError extends Error {}

export class UserInfoService {
  saveUser(user: HttpUser): void {
    const key = user.username;
    void this._userHub.optionalInitLineWithSyncAction(key, async (line) => {
      await this.doSaveUser(user);
      line.next({ user, type: "synced" });
    });
  }

  saveUsers(users: HttpUser[]): void {
    return users.forEach((user) => this.saveUser(user));
  }

  private _getCachedUser(username: string): Promise<User | null> {
    return dataStorage.getItem<HttpUser | null>(`user.${username}`);
  }

  private doSaveUser(user: HttpUser): Promise<void> {
    return dataStorage.setItem<HttpUser>(`user.${user.username}`, user).then();
  }

  getCachedUser(username: string): Promise<User | null> {
    return this._getCachedUser(username);
  }

  syncUser(username: string): Promise<void> {
    return this._userHub.getLineOrCreate(username).sync();
  }

  private _userHub = new DataHub<
    string,
    | { user: User; type: "cache" | "synced" | "offline" }
    | { user?: undefined; type: "notexist" | "offline" }
  >({
    sync: async (key, line) => {
      if (line.value == undefined) {
        const cache = await this._getCachedUser(key);
        if (cache != null) {
          line.next({ user: cache, type: "cache" });
        }
      }

      try {
        const res = await getHttpUserClient().get(key);
        await this.doSaveUser(res);
        line.next({ user: res, type: "synced" });
      } catch (e) {
        if (e instanceof HttpUserNotExistError) {
          line.next({ type: "notexist" });
        } else {
          const cache = await this._getCachedUser(key);
          line.next({ user: cache ?? undefined, type: "offline" });
          throwIfNotNetworkError(e);
        }
      }
    },
  });

  getUser$(username: string): Observable<User> {
    return this._userHub.getObservable(username).pipe(
      map((state) => state?.user),
      filter((user): user is User => user != null)
    );
  }

  private _getCachedAvatar(username: string): Promise<BlobWithEtag | null> {
    return dataStorage.getItem<BlobWithEtag | null>(`user.${username}.avatar`);
  }

  private saveAvatar(username: string, data: BlobWithEtag): Promise<void> {
    return dataStorage
      .setItem<BlobWithEtag>(`user.${username}.avatar`, data)
      .then();
  }

  getCachedAvatar(username: string): Promise<Blob | null> {
    return this._getCachedAvatar(username).then((d) => d?.data ?? null);
  }

  syncAvatar(username: string): Promise<void> {
    return this._avatarHub.getLineOrCreate(username).sync();
  }

  private _avatarHub = new DataHub<
    string,
    | { data: Blob; type: "cache" | "synced" | "offline" }
    | { data?: undefined; type: "notexist" | "offline" }
  >({
    sync: async (key, line) => {
      const cache = await this._getCachedAvatar(key);
      if (line.value == null) {
        if (cache != null) {
          line.next({ data: cache.data, type: "cache" });
        }
      }

      if (cache == null) {
        try {
          const avatar = await getHttpUserClient().getAvatar(key);
          await this.saveAvatar(key, avatar);
          line.next({ data: avatar.data, type: "synced" });
        } catch (e) {
          line.next({ type: "offline" });
          throwIfNotNetworkError(e);
        }
      } else {
        try {
          const res = await getHttpUserClient().getAvatar(key, cache.etag);
          if (res instanceof NotModified) {
            line.next({ data: cache.data, type: "synced" });
          } else {
            const avatar = res;
            await this.saveAvatar(key, avatar);
            line.next({ data: avatar.data, type: "synced" });
          }
        } catch (e) {
          line.next({ data: cache.data, type: "offline" });
          throwIfNotNetworkError(e);
        }
      }
    },
  });

  getAvatar$(username: string): Observable<Blob> {
    return this._avatarHub.getObservable(username).pipe(
      map((state) => state.data),
      filter((blob): blob is Blob => blob != null)
    );
  }

  getUserInfo(username: string): Observable<User> {
    return from(getHttpUserClient().get(username)).pipe(
      convertError(HttpUserNotExistError, UserNotExistError)
    );
  }

  async setAvatar(username: string, blob: Blob): Promise<void> {
    await getHttpUserClient().putAvatar(username, blob);
    this._avatarHub.getLine(username)?.next({ data: blob, type: "synced" });
  }

  async setNickname(username: string, nickname: string): Promise<void> {
    return getHttpUserClient()
      .patch(username, { nickname })
      .then((user) => {
        this.saveUser(user);
      });
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

    const subscription = userInfoService
      .getAvatar$(username)
      .subscribe((blob) => {
        setState(blob);
      });
    return () => {
      subscription.unsubscribe();
    };
  }, [username]);
  return state;
}
