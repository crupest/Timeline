import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

import { MockActivatedRoute } from 'src/app/test-utilities/activated-route.mock';
import { createMockInternalUserService } from '../internal-user-service/internal-user.service.mock';

import { UserLoginSuccessComponent } from './user-login-success.component';
import { InternalUserService } from '../internal-user-service/internal-user.service';


describe('UserLoginSuccessComponent', () => {
  let component: UserLoginSuccessComponent;
  let fixture: ComponentFixture<UserLoginSuccessComponent>;

  let mockInternalUserService: jasmine.SpyObj<InternalUserService>;
  let mockActivatedRoute: MockActivatedRoute;

  const mockUserInfo = {
    username: 'crupest',
    roles: ['superman', 'coder']
  };

  beforeEach(async(() => {
    mockInternalUserService = createMockInternalUserService();
    mockActivatedRoute = new MockActivatedRoute();

    // mock currentUserInfo property. because it only has a getter so cast it to any first.
    (<any>mockInternalUserService).currentUserInfo = mockUserInfo;

    TestBed.configureTestingModule({
      declarations: [UserLoginSuccessComponent],
      providers: [
        { provide: InternalUserService, useValue: mockInternalUserService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserLoginSuccessComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('user info should work well', () => {
    fixture.detectChanges();

    expect((fixture.debugElement.query(By.css('p.login-success-message')))).toBeFalsy();

    expect((fixture.debugElement.query(By.css('span.username')).nativeElement as HTMLSpanElement).textContent)
      .toBe(mockUserInfo.username);
    expect((fixture.debugElement.query(By.css('span.roles')).nativeElement as HTMLSpanElement).textContent)
      .toBe(mockUserInfo.roles.join(', '));
  });

  it('login success message should display well', () => {
    mockActivatedRoute.pushSnapshotWithParamMap({ fromlogin: 'true' });
    fixture.detectChanges();
    expect((fixture.debugElement.query(By.css('p.login-success-message')))).toBeTruthy();
  });
});
