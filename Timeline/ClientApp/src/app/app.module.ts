import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  MatMenuModule, MatIconModule, MatButtonModule, MatToolbarModule, MatListModule,
  MatProgressBarModule, MatCardModule, MatDialogModule, MatInputModule, MatFormFieldModule, MatProgressSpinnerModule
} from '@angular/material';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';

import { DebounceClickDirective } from './debounce-click.directive';

import { TodoListPageComponent } from './todo/todo-list-page/todo-list-page.component';
import { TodoItemComponent } from './todo/todo-item/todo-item.component';

import { UserDialogComponent } from './user/user-dialog/user-dialog.component';
import { UserLoginComponent } from './user/user-login/user-login.component';
import { UserLoginSuccessComponent } from './user/user-login-success/user-login-success.component';

const importedMatModules = [
  MatMenuModule, MatIconModule, MatButtonModule, MatToolbarModule,
  MatListModule, MatProgressBarModule, MatCardModule, MatDialogModule,
  MatInputModule, MatFormFieldModule, MatProgressSpinnerModule
];

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    TodoListPageComponent,
    TodoItemComponent,
    UserDialogComponent,
    DebounceClickDirective,
    UserLoginComponent,
    UserLoginSuccessComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    ...importedMatModules,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'todo', component: TodoListPageComponent }
    ])
  ],
  entryComponents: [UserDialogComponent],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
