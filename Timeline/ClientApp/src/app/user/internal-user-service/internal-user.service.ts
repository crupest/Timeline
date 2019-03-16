import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { Observable, throwError, BehaviorSubject, of } from 'rxjs';
import { map, catchError, retry, switchMap, tap } from 'rxjs/operators';

import { AlreadyLoginError, BadCredentialsError, BadNetworkError, UnknownError } from './errors';
import {
  createTokenUrl, validateTokenUrl, CreateTokenRequest,
  CreateTokenResponse, ValidateTokenRequest, ValidateTokenResponse
} from './http-entities';
import { UserCredentials, UserInfo } from '../entities';
import { MatSnackBar } from '@angular/material';


/**
 * This service is only used internal in user module.
 */
@Injectable({
  providedIn: 'root'
})
export class InternalUserService {

  private token: string | null = null;
  private userInfoSubject = new BehaviorSubject<UserInfo | null>(null);

  get currentUserInfo(): UserInfo | null {
    return this.userInfoSubject.value;
  }

  get userInfo$(): Observable<UserInfo | null> {
    return this.userInfoSubject;
  }

  constructor(private httpClient: HttpClient, private router: Router, private snackBar: MatSnackBar) {
    const savedToken = window.localStorage.getItem('token');
    if (savedToken === null) {
      setTimeout(() => snackBar.open('No login before!', 'ok', { duration: 2000 }), 0);
    } else {
      this.validateToken(savedToken).subscribe(result => {
        if (result === null) {
          window.localStorage.removeItem('token');
          setTimeout(() => snackBar.open('Last login is no longer invalid!', 'ok', { duration: 2000 }), 0);
        } else {
          this.token = savedToken;
          this.userInfoSubject.next(result);
          setTimeout(() => snackBar.open('You have login already!', 'ok', { duration: 2000 }), 0);
        }
      }, _ => {
        setTimeout(() => snackBar.open('Failed to check last login', 'ok', { duration: 2000 }), 0);
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

  tryLogin(credentials: UserCredentials, options: { remember: boolean } = { remember: true }): Observable<UserInfo> {
    if (this.token) {
      return throwError(new AlreadyLoginError());
    }

    return this.httpClient.post<CreateTokenResponse>(createTokenUrl, <CreateTokenRequest>credentials).pipe(
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
        if (options.remember) {
          window.localStorage.setItem('token', result.token);
        }
        this.userInfoSubject.next(result.userInfo);
        return result.userInfo;
      })
    );
  }
}
