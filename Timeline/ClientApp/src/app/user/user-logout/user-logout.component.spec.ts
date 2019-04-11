import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UserLogoutComponent } from './user-logout.component';
import { InternalUserService } from '../internal-user-service/internal-user.service';

describe('UserLogoutComponent', () => {
  let component: UserLogoutComponent;
  let fixture: ComponentFixture<UserLogoutComponent>;

  let mockInternalUserService: jasmine.SpyObj<InternalUserService>;

  beforeEach(async(() => {
    mockInternalUserService = jasmine.createSpyObj('InternalUserService', ['logout']);

    TestBed.configureTestingModule({
      declarations: [UserLogoutComponent],
      providers: [{ provide: InternalUserService, useValue: mockInternalUserService }]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserLogoutComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should logout on init', () => {
    fixture.detectChanges();
    expect(mockInternalUserService.logout).toHaveBeenCalled();
  });
});
