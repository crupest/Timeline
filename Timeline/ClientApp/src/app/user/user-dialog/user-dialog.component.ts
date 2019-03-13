import { Component, OnInit, ViewChild } from '@angular/core';
import { RouterOutlet, Router, ActivationStart } from '@angular/router';

@Component({
  selector: 'app-user-dialog',
  templateUrl: './user-dialog.component.html',
  styleUrls: ['./user-dialog.component.css']
})
export class UserDialogComponent implements OnInit {

  constructor(private router: Router) { }

  @ViewChild(RouterOutlet) outlet!: RouterOutlet;

  ngOnInit() {
    // this is a workaround for a bug. see https://github.com/angular/angular/issues/20694
    const subscription = this.router.events.subscribe(e => {
      if (e instanceof ActivationStart && e.snapshot.outlet === 'user') {
        this.outlet.deactivate();
        subscription.unsubscribe();
      }
    });
  }
}
