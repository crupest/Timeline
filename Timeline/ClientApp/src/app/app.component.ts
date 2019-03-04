import { Component } from '@angular/core';
import { MatDialog } from '@angular/material';
import { UserDialogComponent } from './user-dialog/user-dialog.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  constructor(private dialog: MatDialog) { }

  openUserDialog() {
    this.dialog.open(UserDialogComponent, {
      width: '250px'
    });
  }
}
