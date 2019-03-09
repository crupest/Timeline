import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { Observable, of, throwError, BehaviorSubject } from 'rxjs';
import { map, catchError, retry } from 'rxjs/operators';

import { AlreadyLoginError, BadCredentialsError, BadNetworkError, UnknownError } from './errors';
import { CreateTokenRequest, CreateTokenResponse, ValidateTokenRequest, ValidateTokenResponse } from './http-entities';
import { UserCredentials, UserInfo } from '../entities';


export type UserLoginState = 'nologin' | 'invalidlogin' | 'success';

/**
 * This service is only used internal in user module.
 */
@Injectable({
  providedIn: 'root'
})
export class InternalUserService {

  private token: string;
  private userInfoSubject = new BehaviorSubject<UserInfo | null>(null);

  get currentUserInfo(): UserInfo | null {
    return this.userInfoSubject.value;
  }

  get userInfo$(): Observable<UserInfo | null> {
    return this.userInfoSubject;
  }

  constructor(private httpClient: HttpClient, private router: Router) { }

  userRouteNavigate(commands: any[]) {
    this.router.navigate([{
      outlets: {
        user: commands
      }
    }]);
  }

  refreshAndGetUserState(): Observable<UserLoginState> {
    if (this.token === undefined || this.token === null) {
      return of(<UserLoginState>'nologin');
    }

    return this.httpClient.post<ValidateTokenResponse>('/api/User/ValidateToken', <ValidateTokenRequest>{ token: this.token }).pipe(
      retry(3),
      catchError(error => {
        console.error('Failed to validate token.');
        return throwError(error);
      }),
      map(result => {
        if (result.isValid) {
          this.userInfoSubject.next(result.userInfo);
          return <UserLoginState>'success';
        } else {
          this.token = null;
          this.userInfoSubject.next(null);
          return <UserLoginState>'invalidlogin';
        }
      })
    );
  }

  tryLogin(credentials: UserCredentials): Observable<UserInfo> {
    if (this.token) {
      return throwError(new AlreadyLoginError());
    }

    return this.httpClient.post<CreateTokenResponse>('/api/User/CreateToken', <CreateTokenRequest>credentials).pipe(
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
        this.userInfoSubject.next(result.userInfo);
        return result.userInfo;
      })
    );
  }
}
