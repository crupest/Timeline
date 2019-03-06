import { Component, Output, EventEmitter } from '@angular/core';
import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import { delay } from 'rxjs/operators';

import { UserInfo } from '../user-info';
import { UserDialogComponent } from './user-dialog.component';
import { UserService, UserLoginState } from '../user-service/user.service';
import { LoginEvent } from '../user-login/user-login.component';

@Component({
  /* tslint:disable-next-line:component-selector*/
  selector: 'mat-progress-spinner',
  template: ''
})
class MatProgressSpinnerStubComponent { }

@Component({
  selector: 'app-user-login',
  /* tslint:disable-next-line:use-input-property-decorator*/
  inputs: ['message'],
  template: ''
})
class UserLoginStubComponent {
  @Output()
  login = new EventEmitter<LoginEvent>();
}

@Component({
  selector: 'app-user-login-success',
  /* tslint:disable-next-line:use-input-property-decorator*/
  inputs: ['userInfo', 'displayLoginSuccessMessage'],
  template: ''
})
class UserLoginSuccessStubComponent { }

describe('UserDialogComponent', () => {
  let component: UserDialogComponent;
  let fixture: ComponentFixture<UserDialogComponent>;
  let mockUserService: jasmine.SpyObj<UserService>;

  beforeEach(async(() => {
    mockUserService = jasmine.createSpyObj('UserService', ['validateUserLoginState', 'tryLogin']);

    TestBed.configureTestingModule({
      declarations: [UserDialogComponent, MatProgressSpinnerStubComponent,
        UserLoginStubComponent, UserLoginSuccessStubComponent],
      providers: [{ provide: UserService, useValue: mockUserService }]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserDialogComponent);
    component = fixture.componentInstance;
  });

  it('progress spinner should work well', fakeAsync(() => {
    mockUserService.validateUserLoginState.and.returnValue(of(<UserLoginState>{ state: 'nologin' }).pipe(delay(10)));
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('mat-progress-spinner'))).toBeTruthy();
    tick(10);
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('mat-progress-spinner'))).toBeFalsy();
  }));

  it('nologin should work well', () => {
    mockUserService.validateUserLoginState.and.returnValue(of(<UserLoginState>{ state: 'nologin' }));

    fixture.detectChanges();

    expect(mockUserService.validateUserLoginState).toHaveBeenCalled();
    expect(fixture.debugElement.query(By.css('app-user-login'))).toBeTruthy();
    expect(fixture.debugElement.query(By.css('app-user-login-success'))).toBeFalsy();
  });

  it('success should work well', () => {
    mockUserService.validateUserLoginState.and.returnValue(of(<UserLoginState>{ state: 'success', userInfo: {} }));

    fixture.detectChanges();

    expect(mockUserService.validateUserLoginState).toHaveBeenCalled();
    expect(fixture.debugElement.query(By.css('app-user-login'))).toBeFalsy();
    expect(fixture.debugElement.query(By.css('app-user-login-success'))).toBeTruthy();
  });

  it('login should work well', () => {
    mockUserService.validateUserLoginState.and.returnValue(of(<UserLoginState>{ state: 'nologin' }));

    fixture.detectChanges();
    expect(mockUserService.validateUserLoginState).toHaveBeenCalled();
    expect(fixture.debugElement.query(By.css('app-user-login'))).toBeTruthy();
    expect(fixture.debugElement.query(By.css('app-user-login-success'))).toBeFalsy();

    mockUserService.tryLogin.withArgs('user', 'user').and.returnValue(of(<UserInfo>{
      username: 'user',
      roles: ['user']
    }));

    (fixture.debugElement.query(By.css('app-user-login')).componentInstance as
      UserLoginStubComponent).login.emit(<LoginEvent>{
        username: 'user',
        password: 'user'
      });

    fixture.detectChanges();

    expect(mockUserService.tryLogin).toHaveBeenCalledWith('user', 'user');

    expect(fixture.debugElement.query(By.css('app-user-login'))).toBeFalsy();
    expect(fixture.debugElement.query(By.css('app-user-login-success'))).toBeTruthy();
  });
});
