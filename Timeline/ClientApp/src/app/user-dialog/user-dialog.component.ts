import { Component, OnInit } from '@angular/core';
import { UserService, UserInfo } from './user.service';
import { LoginEvent, LoginMessage } from '../user-login/user-login.component';

@Component({
  selector: 'app-user-dialog',
  templateUrl: './user-dialog.component.html',
  styleUrls: ['./user-dialog.component.css']
})
export class UserDialogComponent implements OnInit {

  constructor(private userService: UserService) { }

  state: 'loading' | 'login' | 'success' = 'loading';

  loginMessage: LoginMessage;

  userInfo: UserInfo;

  ngOnInit() {
    this.userService.validateUserLoginState().subscribe(result => {
      if (result.state === 'success') {
        this.userInfo = result.userInfo;
        this.state = 'success';
      } else {
        if (result.state === 'invalid') {
          this.loginMessage = 'invalidlogin';
        } else if (result.state === 'nologin') {
          this.loginMessage = 'nologin';
        }
        this.state = 'login';
      }
    });
  }

  login(event: LoginEvent) {
    this.userService.tryLogin(event.username, event.password).subscribe(result => {
      this.userInfo = result;
      this.state = 'success';
    }, (error: Error) => {
      this.loginMessage = error.message;
    });
  }
}
