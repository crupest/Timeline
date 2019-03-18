import { Component, OnInit } from '@angular/core';
import { InternalUserService } from '../internal-user-service/internal-user.service';

@Component({
  selector: 'app-user-logout',
  templateUrl: './user-logout.component.html',
  styleUrls: ['./user-logout.component.css']
})
export class UserLogoutComponent implements OnInit {

  constructor(private userService: InternalUserService) { }

  ngOnInit() {
    this.userService.logout();
  }

}
