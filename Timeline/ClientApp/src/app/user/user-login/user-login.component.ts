import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import { InternalUserService } from '../internal-user-service/internal-user.service';

export type LoginMessage = 'nologin' | 'invalidlogin' | string;


@Component({
  selector: 'app-user-login',
  templateUrl: './user-login.component.html',
  styleUrls: ['./user-login.component.css']
})
export class UserLoginComponent implements OnInit {

  constructor(private route: ActivatedRoute, private userService: InternalUserService) { }

  message: LoginMessage;

  form = new FormGroup({
    username: new FormControl(''),
    password: new FormControl('')
  });

  ngOnInit() {
    this.message = this.route.snapshot.paramMap.get('reason');
  }

  onLoginButtonClick() {
    this.userService.tryLogin(this.form.value).subscribe(_ => {
      this.userService.userRouteNavigate(['success', { reason: 'login' }]);
    }, (error: Error) => this.message = error.message);
  }
}
