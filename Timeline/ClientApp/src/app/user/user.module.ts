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
import { UtilityModule } from '../utility/utility.module';

@NgModule({
  declarations: [UserDialogComponent, UserLoginComponent, UserLoginSuccessComponent],
  imports: [
    CommonModule, HttpClientModule, ReactiveFormsModule,
    MatFormFieldModule, MatProgressSpinnerModule, MatDialogModule, MatInputModule, MatButtonModule,
    UtilityModule
  ],
  entryComponents: [UserDialogComponent]
})
export class UserModule { }
