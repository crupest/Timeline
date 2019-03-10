import { Component } from '@angular/core';
import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router, Event } from '@angular/router';
import { of, Observable } from 'rxjs';
import { delay } from 'rxjs/operators';

import { UserDialogComponent } from './user-dialog.component';
import { createMockInternalUserService } from '../internal-user-service/internal-user.service.mock';
import { InternalUserService, UserLoginState } from '../internal-user-service/internal-user.service';

@Component({
  /* tslint:disable-next-line:component-selector*/
  selector: 'mat-progress-spinner',
  template: ''
})
class MatProgressSpinnerStubComponent { }

@Component({
  /* tslint:disable-next-line:component-selector*/
  selector: 'router-outlet',
  template: ''
})
class RouterOutletStubComponent { }


describe('UserDialogComponent', () => {
  let component: UserDialogComponent;
  let fixture: ComponentFixture<UserDialogComponent>;
  let mockInternalUserService: jasmine.SpyObj<InternalUserService>;


  beforeEach(async(() => {
    mockInternalUserService = createMockInternalUserService();

    TestBed.configureTestingModule({
      declarations: [UserDialogComponent, MatProgressSpinnerStubComponent, RouterOutletStubComponent],
      providers: [{ provide: InternalUserService, useValue: mockInternalUserService },
      { // for the workaround
        provide: Router, useValue: {
          events: new Observable<Event>()
        }
      }]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserDialogComponent);
    component = fixture.componentInstance;
  });

  it('progress spinner should work well', fakeAsync(() => {
    mockInternalUserService.refreshAndGetUserState.and.returnValue(of(<UserLoginState>'nologin').pipe(delay(10)));
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('mat-progress-spinner'))).toBeTruthy();
    tick(10);
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('mat-progress-spinner'))).toBeFalsy();
  }));

  it('nologin should work well', () => {
    mockInternalUserService.refreshAndGetUserState.and.returnValue(of(<UserLoginState>'nologin'));

    fixture.detectChanges();

    expect(mockInternalUserService.refreshAndGetUserState).toHaveBeenCalled();
    expect(mockInternalUserService.userRouteNavigate).toHaveBeenCalledWith(['login', { reason: 'nologin' }]);
  });

  it('invalid login should work well', () => {
    mockInternalUserService.refreshAndGetUserState.and.returnValue(of(<UserLoginState>'invalidlogin'));

    fixture.detectChanges();

    expect(mockInternalUserService.refreshAndGetUserState).toHaveBeenCalled();
    expect(mockInternalUserService.userRouteNavigate).toHaveBeenCalledWith(['login',  { reason: 'invalidlogin' }]);
  });

  it('success should work well', () => {
    mockInternalUserService.refreshAndGetUserState.and.returnValue(of(<UserLoginState>'success'));

    fixture.detectChanges();

    expect(mockInternalUserService.refreshAndGetUserState).toHaveBeenCalled();
    expect(mockInternalUserService.userRouteNavigate).toHaveBeenCalledWith(['success', { reason: 'already' }]);
  });
});
