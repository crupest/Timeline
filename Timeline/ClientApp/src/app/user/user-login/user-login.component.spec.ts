import { NO_ERRORS_SCHEMA } from '@angular/core';
import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';

import { UserLoginComponent, LoginEvent } from './user-login.component';

describe('UserLoginComponent', () => {
  let component: UserLoginComponent;
  let fixture: ComponentFixture<UserLoginComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [UserLoginComponent],
      imports: [ReactiveFormsModule],
      schemas: [NO_ERRORS_SCHEMA]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('reactive form should work well', () => {
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

  it('login event should work well', fakeAsync(() => {
    let userCredential: LoginEvent;
    component.login.subscribe((e: LoginEvent) => { userCredential = e; });
    fixture.detectChanges();
    const mockValue = {
      username: 'user',
      password: 'user'
    };
    component.form.setValue(mockValue);
    component.onLoginButtonClick();
    expect(userCredential).toEqual(mockValue);
  }));
});
