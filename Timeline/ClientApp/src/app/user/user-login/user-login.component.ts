import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

import { InternalUserService } from '../internal-user-service/internal-user.service';


export type LoginMessage = 'nologin' | 'invalidlogin' | string | null | undefined;

@Component({
  selector: 'app-user-login',
  templateUrl: './user-login.component.html',
  styleUrls: ['./user-login.component.css']
})
export class UserLoginComponent implements OnInit {

  constructor(private userService: InternalUserService) { }

  message: LoginMessage;

  form = new FormGroup({
    username: new FormControl(''),
    password: new FormControl(''),
    rememberMe: new FormControl(false)
  });

  ngOnInit() {
    if (this.userService.currentUserInfo) {
      throw new Error('Route error! Already login!');
    }
    this.message = 'nologin';
  }

  onLoginButtonClick() {
    this.userService.tryLogin(this.form.value).subscribe(_ => {
      this.userService.userRouteNavigate(['success', { fromlogin: 'true' }]);
    }, (error: Error) => this.message = error.message);
  }
}
