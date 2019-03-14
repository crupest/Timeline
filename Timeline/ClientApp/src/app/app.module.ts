import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatIconModule, MatButtonModule, MatToolbarModule, MatDialogModule } from '@angular/material';

import { AppComponent } from './app.component';

import { TodoModule } from './todo/todo.module';
import { HomeModule } from './home/home.module';
import { UserModule } from './user/user.module';
import { UserService } from './user/user.service';


@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    MatIconModule, MatButtonModule, MatToolbarModule, MatDialogModule,
    HomeModule, TodoModule, UserModule,
    RouterModule.forRoot([
      { path: '', redirectTo: '/home', pathMatch: 'full' },
    ])
  ],
  providers: [UserService],
  bootstrap: [AppComponent]
})
export class AppModule { }
