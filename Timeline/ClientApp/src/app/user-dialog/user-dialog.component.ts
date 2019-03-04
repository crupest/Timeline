import { Component, OnInit } from '@angular/core';
import { UserService, UserInfo } from './user.service';
import { LoginEvent } from '../user-login/user-login.component';

@Component({
  selector: 'app-user-dialog',
  templateUrl: './user-dialog.component.html',
  styleUrls: ['./user-dialog.component.css']
})
export class UserDialogComponent implements OnInit {

  constructor(private userService: UserService) { }

  state: 'loading' | 'login' | 'success' = 'loading';

  loginMessage: string;

  userInfo: UserInfo;

  ngOnInit() {
    this.userService.validateUserLoginState().subscribe(result => {
      if (result.state === 'success') {
        this.userInfo = result.userInfo;
        this.state = 'success';
      } else {
        if (result.state === 'invalid') {
          this.loginMessage = 'Your login is no longer valid';
        } else {
          this.loginMessage = 'You haven\'t logged in.';
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
