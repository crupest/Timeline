import { Component, Output, OnInit, EventEmitter } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { UserService } from '../user-service/user.service';
import { ActivatedRoute } from '@angular/router';

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
export class UserLoginComponent implements OnInit {

  constructor(private route: ActivatedRoute, private userService: UserService) { }

  message: string;

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
