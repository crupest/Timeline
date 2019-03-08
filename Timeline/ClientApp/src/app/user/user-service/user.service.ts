import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError, retry } from 'rxjs/operators';

import { UserCredentials, UserInfo } from '../entities';
import { Router } from '@angular/router';

export interface CreateTokenResult {
  token: string;
  userInfo: UserInfo;
}

export interface TokenValidationRequest {
  token: string;
}

export interface TokenValidationResult {
  isValid: boolean;
  userInfo?: UserInfo;
}

export interface UserLoginState {
  state: 'nologin' | 'invalidlogin' | 'success';
  userInfo?: UserInfo;
}

export class BadNetworkException extends Error {
  constructor() {
    super('Network is bad.');
  }
}

export class AlreadyLoginException extends Error {
  constructor() {
    super('There is already a token saved. Please call validateUserLoginState first.');
  }
}

export class BadCredentialsException extends Error {
  constructor() {
    super(`Username or password is wrong.`);
  }
}

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private token: string;
  userInfo: UserInfo;

  constructor(private httpClient: HttpClient, private router: Router) { }

  userRouteNavigate(commands: any[]) {
    this.router.navigate([{
      outlets: {
        user: commands
      }
    }]);
  }

  validateUserLoginState(): Observable<UserLoginState> {
    if (this.token === undefined || this.token === null) {
      return of(<UserLoginState>{ state: 'nologin' });
    }

    return this.httpClient.post<TokenValidationResult>('/api/User/ValidateToken', <TokenValidationRequest>{ token: this.token }).pipe(
      retry(3),
      catchError(error => {
        console.error('Failed to validate token.');
        return throwError(error);
      }),
      map(result => {
        if (result.isValid) {
          this.userInfo = result.userInfo;
          return <UserLoginState>{
            state: 'success',
            userInfo: result.userInfo
          };
        } else {
          this.token = null;
          this.userInfo = null;
          return <UserLoginState>{
            state: 'invalidlogin'
          };
        }
      })
    );
  }

  tryLogin(credentials: UserCredentials): Observable<UserInfo> {
    if (this.token) {
      return throwError(new AlreadyLoginException());
    }

    return this.httpClient.post<CreateTokenResult>('/api/User/CreateToken', credentials).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.error instanceof ErrorEvent) {
          console.error('An error occurred when login: ' + error.error.message);
          return throwError(new BadNetworkException());
        } else if (error.status === 400) {
          console.error('An error occurred when login: wrong credentials.');
          return throwError(new BadCredentialsException());
        } else {
          console.error('An unknown error occurred when login: ' + error);
          return throwError(error);
        }
      }),
      map(result => {
        this.token = result.token;
        this.userInfo = result.userInfo;
        return result.userInfo;
      })
    );
  }
}
