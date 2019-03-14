import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';

import { InternalUserService } from './internal-user-service/internal-user.service';

export type AuthStrategy = 'all' | 'requirelogin' | 'requirenologin' | string[];

export abstract class AuthGuard implements CanActivate {

  constructor(protected internalUserService: InternalUserService) { }

  onAuthFailed() { }

  abstract get authStrategy(): AuthStrategy;

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot):
    Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    const { authStrategy } = this;

    if (authStrategy === 'all') {
      return true;
    }

    const { currentUserInfo } = this.internalUserService;

    if (currentUserInfo === null) {
      if (authStrategy === 'requirenologin') {
        return true;
      }
    } else {
      if (authStrategy === 'requirelogin') {
        return true;
      } else if (authStrategy instanceof Array) {
        const { roles } = currentUserInfo;
        if (authStrategy.every(value => roles.includes(value))) {
          return true;
        }
      }
    }

    // reach here means auth fails
    this.onAuthFailed();
    return false;
  }
}

@Injectable({
  providedIn: 'root'
})
export class RequireLoginGuard extends AuthGuard {
  readonly authStrategy: AuthStrategy = 'requirelogin';

  // never remove this constructor or you will get an injection error.
  constructor(internalUserService: InternalUserService) {
    super(internalUserService);
  }

  onAuthFailed() {
    this.internalUserService.userRouteNavigate(['login']);
  }
}

@Injectable({
  providedIn: 'root'
})
export class RequireNoLoginGuard extends AuthGuard {
  readonly authStrategy: AuthStrategy = 'requirenologin';

  // never remove this constructor or you will get an injection error.
  constructor(internalUserService: InternalUserService) {
    super(internalUserService);
  }

  onAuthFailed() {
    this.internalUserService.userRouteNavigate(['success']);
  }
}
