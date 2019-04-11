import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { Observable, throwError, BehaviorSubject, of } from 'rxjs';
import { map, catchError, retry, switchMap, tap, filter } from 'rxjs/operators';

import { AlreadyLoginError, BadCredentialsError, BadNetworkError, UnknownError } from './errors';
import {
  createTokenUrl, validateTokenUrl, CreateTokenRequest,
  CreateTokenResponse, ValidateTokenRequest, ValidateTokenResponse
} from './http-entities';
import { UserCredentials, UserInfo } from '../entities';
import { MatSnackBar } from '@angular/material';
import { WINDOW } from '../window-inject-token';

export const snackBarText = {
  checkFail: 'Failed to check last login',
  noLogin: 'No login before!',
  alreadyLogin: 'You have login already!',
  invalidLogin: 'Last login is no longer invalid!',
  ok: 'ok'
};

export type SnackBarTextKey = Exclude<keyof typeof snackBarText, 'ok'>;

export const TOKEN_STORAGE_KEY = 'token';

export interface LoginInfo extends UserCredentials {
  rememberMe: boolean;
}

/**
 * This service is only used internal in user module.
 */
@Injectable({
  providedIn: 'root'
})
export class InternalUserService {

  private token: string | null = null;
  private userInfoSubject = new BehaviorSubject<UserInfo | null | undefined>(undefined);

  readonly userInfo$: Observable<UserInfo | null> =
    <Observable<UserInfo | null>>this.userInfoSubject.pipe(filter(value => value !== undefined));

  get currentUserInfo(): UserInfo | null | undefined {
    return this.userInfoSubject.value;
  }

  private openSnackBar(snackBar: MatSnackBar, textKey: SnackBarTextKey) {
    setTimeout(() => snackBar.open(snackBarText[textKey], snackBarText.ok, { duration: 2000 }), 0);
  }

  constructor(@Inject(WINDOW) private window: Window, private httpClient: HttpClient, private router: Router, snackBar: MatSnackBar) {
    const savedToken = this.window.localStorage.getItem(TOKEN_STORAGE_KEY);
    if (savedToken === null) {
      this.openSnackBar(snackBar, 'noLogin');
      this.userInfoSubject.next(null);
    } else {
      this.validateToken(savedToken).subscribe(result => {
        if (result === null) {
          this.window.localStorage.removeItem(TOKEN_STORAGE_KEY);
          this.openSnackBar(snackBar, 'invalidLogin');
          this.userInfoSubject.next(null);
        } else {
          this.token = savedToken;
          this.userInfoSubject.next(result);
          this.openSnackBar(snackBar, 'alreadyLogin');
        }
      }, _ => {
        this.openSnackBar(snackBar, 'checkFail');
        this.userInfoSubject.next(null);
      });
    }
  }

  private validateToken(token: string): Observable<UserInfo | null> {
    return this.httpClient.post<ValidateTokenResponse>(validateTokenUrl, <ValidateTokenRequest>{ token: token }).pipe(
      retry(3),
      switchMap(result => {
        if (result.isValid) {
          const { userInfo } = result;
          if (userInfo) {
            return of(userInfo);
          } else {
            return throwError(new Error('Wrong server response. IsValid is true but UserInfo is null.'));
          }
        } else {
          return of(null);
        }
      }),
      tap({
        error: error => {
          console.error('Failed to validate token.');
          console.error(error);
        }
      }),
    );
  }

  userRouteNavigate(commands: any[] | null) {
    this.router.navigate([{
      outlets: {
        user: commands
      }
    }]);
  }

  tryLogin(info: LoginInfo): Observable<UserInfo> {
    if (this.token) {
      return throwError(new AlreadyLoginError());
    }

    return this.httpClient.post<CreateTokenResponse>(createTokenUrl, <CreateTokenRequest>info).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.error instanceof ErrorEvent) {
          console.error('An error occurred when login: ' + error.error.message);
          return throwError(new BadNetworkError());
        } else if (error.status === 400) {
          console.error('An error occurred when login: wrong credentials.');
          return throwError(new BadCredentialsError());
        } else {
          console.error('An unknown error occurred when login: ' + error);
          return throwError(new UnknownError(error));
        }
      }),
      map(result => {
        this.token = result.token;
        if (info.rememberMe) {
          this.window.localStorage.setItem(TOKEN_STORAGE_KEY, result.token);
        }
        this.userInfoSubject.next(result.userInfo);
        return result.userInfo;
      })
    );
  }

  logout() {
    if (this.currentUserInfo === null) {
      throw new Error('No login now. You can\'t logout.');
    }

    this.window.localStorage.removeItem(TOKEN_STORAGE_KEY);
    this.token = null;
    this.userInfoSubject.next(null);
  }
}
