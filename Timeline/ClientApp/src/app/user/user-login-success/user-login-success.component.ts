import { Component, OnInit, Input } from '@angular/core';
import { UserInfo } from '../entities';
import { UserService } from '../user-service/user.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-user-login-success',
  templateUrl: './user-login-success.component.html',
  styleUrls: ['./user-login-success.component.css']
})
export class UserLoginSuccessComponent implements OnInit {

  displayLoginSuccessMessage = false;

  userInfo: UserInfo;

  constructor(private route: ActivatedRoute, private userService: UserService) { }

  ngOnInit() {
    this.userInfo = this.userService.userInfo;
    this.displayLoginSuccessMessage = this.route.snapshot.paramMap.get('reason') === 'login';
  }
}
