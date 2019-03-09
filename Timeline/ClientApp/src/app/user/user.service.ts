import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material';

import { Observable } from 'rxjs';

import { UserInfo } from './entities';
import { InternalUserService } from './internal-user-service/internal-user.service';
import { UserDialogComponent } from './user-dialog/user-dialog.component';


/**
 * This service provides public api of user module.
 */
@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private dialog: MatDialog, private internalService: InternalUserService) { }

  get currentUserInfo(): UserInfo | null {
    return this.internalService.currentUserInfo;
  }

  get userInfo$(): Observable<UserInfo | null> {
    return this.internalService.userInfo$;
  }

  openUserDialog() {
    this.dialog.open(UserDialogComponent, {
      width: '300px'
    });
  }
}
