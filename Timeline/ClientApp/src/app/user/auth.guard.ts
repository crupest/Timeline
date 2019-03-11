import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';

import { UserService } from './user.service';

export type RequiredAuthData = 'all' | 'requirelogin' | 'requirenologin' | string[];

export abstract class AuthGuard implements CanActivate {

  constructor(private userService: UserService) { }

  abstract get requiredAuth(): RequiredAuthData;

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot):
    Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    const { requiredAuth } = this;

    if (requiredAuth === 'all') {
      return true;
    }
    const { currentUserInfo } = this.userService;

    if (currentUserInfo === null) {
      return requiredAuth === 'requirenologin';
    } else {
      if (requiredAuth === 'requirelogin') {
        return true;
      } else if (requiredAuth === 'requirenologin') {
        return false;
      } else {
        const { roles } = currentUserInfo;
        return requiredAuth.every(value => roles.includes(value));
      }
    }
  }
}

@Injectable({
  providedIn: 'root'
})
export class RequireLoginGuard extends AuthGuard {
  readonly requiredAuth: RequiredAuthData = 'requirelogin';

  // never remove this constructor or you will get an injection error.
  constructor(userService: UserService) {
    super(userService);
   }
}

@Injectable({
  providedIn: 'root'
})
export class RequireNoLoginGuard extends AuthGuard {
  readonly requiredAuth: RequiredAuthData = 'requirenologin';

  // never remove this constructor or you will get an injection error.
  constructor(userService: UserService) {
    super(userService);
   }
}
