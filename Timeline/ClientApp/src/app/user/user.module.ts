import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import {
  MatFormFieldModule, MatProgressSpinnerModule,
  MatDialogModule, MatInputModule, MatButtonModule
} from '@angular/material';

import { UserDialogComponent } from './user-dialog/user-dialog.component';
import { UserLoginComponent } from './user-login/user-login.component';
import { UserLoginSuccessComponent } from './user-login-success/user-login-success.component';
import { UtilityModule } from '../utilities/utility.module';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

@NgModule({
  declarations: [UserDialogComponent, UserLoginComponent, UserLoginSuccessComponent],
  imports: [
    RouterModule.forChild([
      { path: 'login', component: UserLoginComponent, outlet: 'user' },
      { path: 'success', component: UserLoginSuccessComponent, outlet: 'user' }
    ]),
    CommonModule, HttpClientModule, ReactiveFormsModule, BrowserAnimationsModule,
    MatFormFieldModule, MatProgressSpinnerModule, MatDialogModule, MatInputModule, MatButtonModule,
    UtilityModule
  ],
  exports: [RouterModule],
  entryComponents: [UserDialogComponent]
})
export class UserModule { }
