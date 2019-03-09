import { TestBed } from '@angular/core/testing';
import { HttpRequest } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { UserInfo, UserCredentials } from '../entities';
import {
  InternalUserService, CreateTokenResult,
  UserLoginState, TokenValidationRequest, TokenValidationResult
} from './internal-user.service';

describe('UserService', () => {
  const tokenCreateUrl = '/api/User/CreateToken';

  const mockUserCredentials: UserCredentials = {
    username: 'user',
    password: 'user'
  };

  beforeEach(() => TestBed.configureTestingModule({
    imports: [HttpClientTestingModule]
  }));

  it('should be created', () => {
    const service: InternalUserService = TestBed.get(InternalUserService);
    expect(service).toBeTruthy();
  });

  it('should be nologin at first', () => {
    const service: InternalUserService = TestBed.get(InternalUserService);
    service.refreshAndGetUserState().subscribe(result => {
      expect(result.state).toBe('nologin');
    });
  });

  it('login should work well', () => {
    const service: InternalUserService = TestBed.get(InternalUserService);

    const mockUserInfo: UserInfo = {
      username: 'user',
      roles: ['user', 'other']
    };

    service.tryLogin(mockUserCredentials).subscribe(result => {
      expect(result).toEqual(mockUserInfo);
    });

    const httpController = TestBed.get(HttpTestingController) as HttpTestingController;

    httpController.expectOne((request: HttpRequest<UserCredentials>) =>
      request.url === tokenCreateUrl &&
      request.body.username === 'user' &&
      request.body.password === 'user').flush(<CreateTokenResult>{
        token: 'test-token',
        userInfo: mockUserInfo
      });

    httpController.verify();
  });

  describe('validateUserLoginState', () => {
    let service: InternalUserService;
    let httpController: HttpTestingController;

    const mockUserInfo: UserInfo = {
      username: 'user',
      roles: ['user', 'other']
    };

    const mockToken = 'mock-token';

    const tokenValidateRequestMatcher = (req: HttpRequest<TokenValidationRequest>) => {
      return req.url === '/api/User/ValidateToken' && req.body.token === mockToken;
    };

    beforeEach(() => {
      service = TestBed.get(InternalUserService);
      httpController = TestBed.get(HttpTestingController);

      service.tryLogin(mockUserCredentials).subscribe(); // subscribe to activate login

      httpController.expectOne(tokenCreateUrl).flush(<CreateTokenResult>{
        token: mockToken,
        userInfo: mockUserInfo
      });
    });

    it('success should work well', () => {
      service.refreshAndGetUserState().subscribe((result: UserLoginState) => {
        expect(result).toEqual(<UserLoginState>{
          state: 'success',
          userInfo: mockUserInfo
        });
      });

      httpController.expectOne(tokenValidateRequestMatcher).flush(<TokenValidationResult>{
        isValid: true,
        userInfo: mockUserInfo
      });

      httpController.verify();
    });

    it('invalid should work well', () => {
      service.refreshAndGetUserState().subscribe((result: UserLoginState) => {
        expect(result).toEqual(<UserLoginState>{
          state: 'invalidlogin'
        });
      });

      httpController.expectOne(tokenValidateRequestMatcher).flush(<TokenValidationResult>{
        isValid: false
      });

      httpController.verify();
    });
  });

  // TODO: test on error situations.
});
