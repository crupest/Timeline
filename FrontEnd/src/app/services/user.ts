import { useState, useEffect } from "react";
import { BehaviorSubject, Observable } from "rxjs";

import { UiLogicError } from "@/common";

import { HttpNetworkError, setHttpToken } from "@/http/common";
import {
  getHttpTokenClient,
  HttpCreateTokenBadCredentialError,
} from "@/http/token";
import { getHttpUserClient, HttpUser, UserPermission } from "@/http/user";

import { pushAlert } from "./alert";

interface IAuthUser extends HttpUser {
  token: string;
}

export class AuthUser implements IAuthUser {
  constructor(user: HttpUser, public token: string) {
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

    const savedUserString = localStorage.getItem(USER_STORAGE_KEY);

    const savedAuthUserData =
      savedUserString == null
        ? null
        : (JSON.parse(savedUserString) as IAuthUser);

    const savedUser =
      savedAuthUserData == null
        ? null
        : new AuthUser(savedAuthUserData, savedAuthUserData.token);

    if (savedUser == null) {
      this.userSubject.next(null);
      return null;
    }

    this.userSubject.next(savedUser);

    const savedToken = savedUser.token;
    try {
      const res = await getHttpTokenClient().verify({ token: savedToken });
      const user = new AuthUser(res.user, savedToken);
      localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
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
        localStorage.removeItem(USER_STORAGE_KEY);
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
        localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
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

  logout(): Promise<void> {
    if (this.currentUser === undefined) {
      throw new UiLogicError("Please check user first.");
    }
    if (this.currentUser === null) {
      throw new UiLogicError("No login.");
    }
    localStorage.removeItem(USER_STORAGE_KEY);
    this.userSubject.next(null);
    return Promise.resolve();
  }

  changePassword(oldPassword: string, newPassword: string): Promise<void> {
    if (this.currentUser == undefined) {
      throw new UiLogicError("Not login or checked now, can't log out.");
    }

    return getHttpUserClient()
      .changePassword({
        oldPassword,
        newPassword,
      })
      .then(() => this.logout());
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
