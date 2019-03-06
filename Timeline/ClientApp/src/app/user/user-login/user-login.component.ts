import { Component, Output, OnInit, EventEmitter, Input } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

export type LoginMessage = 'nologin' | 'invalidlogin' | string;

export class LoginEvent {
  username: string;
  password: string;
}

@Component({
  selector: 'app-user-login',
  templateUrl: './user-login.component.html',
  styleUrls: ['./user-login.component.css']
})
export class UserLoginComponent {

  @Input()
  message: LoginMessage;

  @Output()
  login = new EventEmitter<LoginEvent>();

  form = new FormGroup({
    username: new FormControl(''),
    password: new FormControl('')
  });

  onLoginButtonClick() {
    this.login.emit(this.form.value);
  }
}
