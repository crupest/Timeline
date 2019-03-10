import { NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

import { of, throwError } from 'rxjs';

import { createMockInternalUserService } from '../internal-user-service/internal-user.service.mock';
import { MockActivatedRoute } from '../../test-utilities/activated-route.mock';
import { UserLoginComponent } from './user-login.component';
import { InternalUserService } from '../internal-user-service/internal-user.service';
import { UserInfo } from '../entities';

describe('UserLoginComponent', () => {
  let component: UserLoginComponent;
  let fixture: ComponentFixture<UserLoginComponent>;
  let mockInternalUserService: jasmine.SpyObj<InternalUserService>;
  let mockActivatedRoute: MockActivatedRoute;

  beforeEach(async(() => {
    mockInternalUserService = createMockInternalUserService();
    mockActivatedRoute = new MockActivatedRoute();

    TestBed.configureTestingModule({
      declarations: [UserLoginComponent],
      providers: [
        { provide: InternalUserService, useValue: mockInternalUserService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ],
      imports: [ReactiveFormsModule],
      schemas: [NO_ERRORS_SCHEMA]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserLoginComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('reactive form should work well', () => {
    fixture.detectChanges();

    const usernameInput = fixture.debugElement.query(By.css('input[type=text]')).nativeElement as HTMLInputElement;
    const passwordInput = fixture.debugElement.query(By.css('input[type=password]')).nativeElement as HTMLInputElement;

    usernameInput.value = 'user';
    usernameInput.dispatchEvent(new Event('input'));
    passwordInput.value = 'user';
    passwordInput.dispatchEvent(new Event('input'));

    fixture.detectChanges();

    expect(component.form.value).toEqual({
      username: 'user',
      password: 'user'
    });
  });

  it('login should work well', () => {
    fixture.detectChanges();

    const mockValue = {
      username: 'user',
      password: 'user'
    };

    mockInternalUserService.tryLogin.withArgs(mockValue).and.returnValue(of(<UserInfo>{ username: 'user', roles: ['user'] }));

    component.form.setValue(mockValue);
    component.onLoginButtonClick();

    expect(mockInternalUserService.tryLogin).toHaveBeenCalledWith(mockValue);
    expect(mockInternalUserService.userRouteNavigate).toHaveBeenCalledWith(['success', { reason: 'login' }]);
  });

  describe('message display', () => {
    it('nologin reason should display', () => {
      mockActivatedRoute.pushSnapshotWithParamMap({ reason: 'nologin' });
      fixture.detectChanges();
      expect(component.message).toBe('nologin');
      expect((fixture.debugElement.query(By.css('p.mat-body')).nativeElement as
        HTMLParagraphElement).textContent).toBe('You haven\'t login.');
    });

    it('invalid login reason should display', () => {
      mockActivatedRoute.pushSnapshotWithParamMap({ reason: 'invalidlogin' });
      fixture.detectChanges();
      expect(component.message).toBe('invalidlogin');
      expect((fixture.debugElement.query(By.css('p.mat-body')).nativeElement as
        HTMLParagraphElement).textContent).toBe('Your login is no longer valid.');
    });

    it('custom error message should display', () => {
      const customMessage = 'custom message';

      fixture.detectChanges();

      const mockValue = {
        username: 'user',
        password: 'user'
      };
      mockInternalUserService.tryLogin.withArgs(mockValue).and.returnValue(throwError(new Error(customMessage)));
      component.form.setValue(mockValue);
      component.onLoginButtonClick();

      fixture.detectChanges();
      expect(component.message).toBe(customMessage);
      expect((fixture.debugElement.query(By.css('p.mat-body')).nativeElement as
        HTMLParagraphElement).textContent).toBe(customMessage);
    });
  });
});
