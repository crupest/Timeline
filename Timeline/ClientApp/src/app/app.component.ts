import { Component } from '@angular/core';

import { UserService } from './user/user.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  // never remove userService because we need it explicit constructing.
  constructor(userService: UserService) { }
}
