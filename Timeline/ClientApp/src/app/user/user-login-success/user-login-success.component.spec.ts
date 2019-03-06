import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UserLoginSuccessComponent } from './user-login-success.component';
import { By } from '@angular/platform-browser';

describe('UserLoginSuccessComponent', () => {
  let component: UserLoginSuccessComponent;
  let fixture: ComponentFixture<UserLoginSuccessComponent>;

  const mockUserInfo = {
    username: 'crupest',
    roles: ['superman', 'coder']
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [UserLoginSuccessComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserLoginSuccessComponent);
    component = fixture.componentInstance;
    component.userInfo = mockUserInfo;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should work well', () => {
    expect((fixture.debugElement.query(By.css('span.username')).nativeElement as HTMLSpanElement).textContent)
      .toBe(mockUserInfo.username);
    expect((fixture.debugElement.query(By.css('span.roles')).nativeElement as HTMLSpanElement).textContent)
      .toBe(mockUserInfo.roles.join(', '));
  });
});
