import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { UserInfo } from '../entities';
import { InternalUserService } from '../internal-user-service/internal-user.service';
import { throwIfNullOrUndefined } from 'src/app/utilities/language-untilities';

@Component({
  selector: 'app-user-login-success',
  templateUrl: './user-login-success.component.html',
  styleUrls: ['./user-login-success.component.css']
})
export class UserLoginSuccessComponent implements OnInit {

  displayLoginSuccessMessage = false;

  userInfo!: UserInfo;

  constructor(private route: ActivatedRoute, private userService: InternalUserService) { }

  ngOnInit() {
    this.userInfo = throwIfNullOrUndefined(this.userService.currentUserInfo, () => 'Route error. No login now!');
    this.displayLoginSuccessMessage = this.route.snapshot.paramMap.get('fromlogin') === 'true';
  }
}
