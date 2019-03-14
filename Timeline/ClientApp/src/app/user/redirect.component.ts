import { Component, OnInit } from '@angular/core';
import { InternalUserService } from './internal-user-service/internal-user.service';

@Component({
  selector: 'app-redirect',
  template: ''
})
export class RedirectComponent implements OnInit {

  constructor(private userService: InternalUserService) { }

  ngOnInit() {
    this.userService.userRouteNavigate(['login']);
  }
}
