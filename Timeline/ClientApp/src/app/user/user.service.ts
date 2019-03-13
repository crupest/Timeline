import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material';
import { Router, ActivationStart } from '@angular/router';

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

  private dialogRef: MatDialogRef<UserDialogComponent> | null = null;

  constructor(router: Router, private dialog: MatDialog, private internalService: InternalUserService) {
    router.events.subscribe(event => {
      if (event instanceof ActivationStart && event.snapshot.outlet === 'user') {
        setTimeout(() => this.openUserDialog(), 0);
      }
    });
  }

  get currentUserInfo(): UserInfo | null {
    return this.internalService.currentUserInfo;
  }

  get userInfo$(): Observable<UserInfo | null> {
    return this.internalService.userInfo$;
  }

  private openUserDialog() {
    if (this.dialogRef) {
      return;
    }

    this.dialogRef = this.dialog.open(UserDialogComponent, {
      width: '300px'
    });

    const subscription = this.dialogRef.afterClosed().subscribe(_ => {
      this.internalService.userRouteNavigate(null);
      this.dialogRef = null;
      subscription.unsubscribe();
    });
  }
}
