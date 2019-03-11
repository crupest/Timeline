import { Component, OnInit, Input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { UserInfo } from '../entities';
import { InternalUserService } from '../internal-user-service/internal-user.service';
import { throwIfFalsy } from 'src/app/utilities/language-untilities';

@Component({
  selector: 'app-user-login-success',
  templateUrl: './user-login-success.component.html',
  styleUrls: ['./user-login-success.component.css']
})
export class UserLoginSuccessComponent implements OnInit {

  displayLoginSuccessMessage = false;

  userInfo: UserInfo | undefined;

  constructor(private route: ActivatedRoute, private userService: InternalUserService) { }

  ngOnInit() {
    const { currentUserInfo } = this.userService;

    if (!currentUserInfo) {
      throw new Error('Route error. No login now!');
    }

    this.userInfo = this.userService.currentUserInfo!;
    this.displayLoginSuccessMessage = this.route.snapshot.paramMap.get('reason') === 'login';
  }
}
