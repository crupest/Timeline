import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { InternalUserService } from '../internal-user-service/internal-user.service';
import { RouterOutlet, Router, ActivationStart } from '@angular/router';

@Component({
  selector: 'app-user-dialog',
  templateUrl: './user-dialog.component.html',
  styleUrls: ['./user-dialog.component.css']
})
export class UserDialogComponent implements OnInit, OnDestroy {

  constructor(private userService: InternalUserService, private router: Router) { }

  @ViewChild(RouterOutlet) outlet!: RouterOutlet;

  isLoading = true;

  ngOnInit() {
    // this is a workaround for a bug. see https://github.com/angular/angular/issues/20694
    const subscription = this.router.events.subscribe(e => {
      if (e instanceof ActivationStart && e.snapshot.outlet === 'user') {
        this.outlet.deactivate();
        subscription.unsubscribe();
      }
    });


    this.userService.refreshAndGetUserState().subscribe(result => {
      this.isLoading = false;
      if (result === 'success') {
        this.userService.userRouteNavigate(['success', { reason: 'already' }]);
      } else {
        this.userService.userRouteNavigate(['login', { reason: result }]);
      }
    });
  }

  ngOnDestroy() {
    this.userService.userRouteNavigate(null);
  }
}
