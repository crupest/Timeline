import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import {
  MatFormFieldModule, MatProgressSpinnerModule,
  MatDialogModule, MatInputModule, MatButtonModule, MatSnackBarModule, MatCheckboxModule
} from '@angular/material';

import { RequireNoLoginGuard, RequireLoginGuard } from './auth.guard';
import { UserDialogComponent } from './user-dialog/user-dialog.component';
import { UserLoginComponent } from './user-login/user-login.component';
import { UserLoginSuccessComponent } from './user-login-success/user-login-success.component';
import { RedirectComponent } from './redirect.component';
import { UtilityModule } from '../utilities/utility.module';
import { WINDOW } from './window-inject-token';
import { UserLogoutComponent } from './user-logout/user-logout.component';

@NgModule({
  declarations: [UserDialogComponent, UserLoginComponent, UserLoginSuccessComponent, RedirectComponent, UserLogoutComponent],
  imports: [
    RouterModule.forChild([
      { path: 'login', canActivate: [RequireNoLoginGuard], component: UserLoginComponent, outlet: 'user' },
      { path: 'success', canActivate: [RequireLoginGuard], component: UserLoginSuccessComponent, outlet: 'user' },
      { path: 'logout', canActivate: [RequireLoginGuard], component: UserLogoutComponent, outlet: 'user' },
      { path: '**', component: RedirectComponent, outlet: 'user' }
    ]),
    CommonModule, HttpClientModule, ReactiveFormsModule, BrowserAnimationsModule,
    MatFormFieldModule, MatProgressSpinnerModule, MatDialogModule, MatInputModule, MatButtonModule, MatCheckboxModule, MatSnackBarModule,
    UtilityModule
  ],
  providers: [{ provide: WINDOW, useValue: window }],
  exports: [RouterModule],
  entryComponents: [UserDialogComponent]
})
export class UserModule { }
