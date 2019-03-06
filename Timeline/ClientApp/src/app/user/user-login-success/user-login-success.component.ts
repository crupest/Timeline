import { Component, OnInit, Input } from '@angular/core';
import { UserInfo } from '../user-info';

@Component({
  selector: 'app-user-login-success',
  templateUrl: './user-login-success.component.html',
  styleUrls: ['./user-login-success.component.css']
})
export class UserLoginSuccessComponent implements OnInit {

  @Input()
  displayLoginSuccessMessage = false;

  @Input()
  userInfo: UserInfo;

  constructor() { }

  ngOnInit() {
  }

}
