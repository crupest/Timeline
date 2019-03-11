import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import {
  MatFormFieldModule, MatProgressSpinnerModule,
  MatDialogModule, MatInputModule, MatButtonModule
} from '@angular/material';

import { RequireNoLoginGuard, RequireLoginGuard } from './auth.guard';
import { UserDialogComponent } from './user-dialog/user-dialog.component';
import { UserLoginComponent } from './user-login/user-login.component';
import { UserLoginSuccessComponent } from './user-login-success/user-login-success.component';
import { UtilityModule } from '../utilities/utility.module';

@NgModule({
  declarations: [UserDialogComponent, UserLoginComponent, UserLoginSuccessComponent],
  imports: [
    RouterModule.forChild([
      { path: 'login', canActivate: [RequireNoLoginGuard], component: UserLoginComponent, outlet: 'user' },
      { path: 'success', canActivate: [RequireLoginGuard], component: UserLoginSuccessComponent, outlet: 'user' }
    ]),
    CommonModule, HttpClientModule, ReactiveFormsModule, BrowserAnimationsModule,
    MatFormFieldModule, MatProgressSpinnerModule, MatDialogModule, MatInputModule, MatButtonModule,
    UtilityModule
  ],
  exports: [RouterModule],
  entryComponents: [UserDialogComponent]
})
export class UserModule { }
