import React, { useState, useEffect } from "react";
import { BehaviorSubject, Observable, from } from "rxjs";

import { UiLogicError } from "@/common";

import {
  HttpNetworkError,
  BlobWithEtag,
  NotModified,
  setHttpToken,
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

import { DataHub2 } from "./DataHub2";
import { dataStorage } from "./common";
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
      setHttpToken(u?.token ?? null);
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
  saveUser(user: HttpUser): Promise<void> {
    return this.userHub.getLine(user.username).save(user);
  }

  saveUsers(users: HttpUser[]): Promise<void> {
    return Promise.all(users.map((user) => this.saveUser(user))).then();
  }

  async getCachedUser(username: string): Promise<HttpUser | null> {
    const user = await this.userHub.getLine(username).getSavedData();
    if (user == null || user === "notexist") return null;
    return user;
  }

  async getCachedUsers(usernames: string[]): Promise<HttpUser[] | null> {
    const users = await Promise.all(
      usernames.map((username) => this.userHub.getLine(username).getSavedData())
    );

    for (const u of users) {
      if (u == null || u === "notexist") {
        return null;
      }
    }

    return users as HttpUser[];
  }

  private generateUserDataStorageKey(username: string): string {
    return `user.${username}`;
  }

  readonly userHub = new DataHub2<string, HttpUser | "notexist">({
    saveData: (username, data) => {
      if (typeof data === "string") return Promise.resolve();
      return dataStorage
        .setItem<HttpUser>(this.generateUserDataStorageKey(username), data)
        .then();
    },
    getSavedData: (username) => {
      return dataStorage.getItem<HttpUser | null>(
        this.generateUserDataStorageKey(username)
      );
    },
    fetchData: async (username) => {
      try {
        return await getHttpUserClient().get(username);
      } catch (e) {
        if (e instanceof HttpUserNotExistError) {
          return "notexist";
        } else if (e instanceof HttpNetworkError) {
          return null;
        }
        throw e;
      }
    },
  });

  private generateAvatarDataStorageKey(username: string): string {
    return `user.${username}.avatar`;
  }

  readonly avatarHub = new DataHub2<string, BlobWithEtag | "notexist">({
    saveData: async (username, data) => {
      if (typeof data === "string") return;
      await dataStorage.setItem<BlobWithEtag>(
        this.generateAvatarDataStorageKey(username),
        data
      );
    },
    getSavedData: (username) =>
      dataStorage.getItem<BlobWithEtag | null>(
        this.generateAvatarDataStorageKey(username)
      ),
    fetchData: async (username, savedData) => {
      try {
        if (savedData == null || savedData === "notexist") {
          return await getHttpUserClient().getAvatar(username);
        } else {
          const res = await getHttpUserClient().getAvatar(
            username,
            savedData.etag
          );
          if (res instanceof NotModified) {
            return savedData;
          } else {
            return res;
          }
        }
      } catch (e) {
        if (e instanceof HttpUserNotExistError) {
          return "notexist";
        } else if (e instanceof HttpNetworkError) {
          return null;
        } else {
          throw e;
        }
      }
    },
  });

  async setAvatar(username: string, blob: Blob): Promise<void> {
    const etag = await getHttpUserClient().putAvatar(username, blob);
    await this.avatarHub.getLine(username).save({ data: blob, etag });
  }

  async setNickname(username: string, nickname: string): Promise<void> {
    return getHttpUserClient()
      .patch(username, { nickname })
      .then((user) => this.saveUser(user));
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

    const subscription = userInfoService.avatarHub
      .getLine(username)
      .getObservalble()
      .subscribe((data) => {
        if (data.data != null && data.data !== "notexist") {
          setState(data.data.data);
        } else {
          setState(undefined);
        }
      });

    return () => {
      subscription.unsubscribe();
    };
  }, [username]);

  return state;
}
