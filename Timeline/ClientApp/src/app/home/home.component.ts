import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

class LoginInfo {
  username = '';
  password = '';
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  loginInfo = new LoginInfo();
  message = '';

  constructor(/* private http: HttpClient */) { }

  ngOnInit() {
  }

  tryLogin() {
    alert('Not implemented!!!');
  }
}
