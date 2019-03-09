import { TestBed } from '@angular/core/testing';
import { HttpRequest } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';

import { UserInfo, UserCredentials } from '../entities';
import { CreateTokenRequest, CreateTokenResponse, ValidateTokenRequest, ValidateTokenResponse } from './http-entities';
import { InternalUserService, UserLoginState } from './internal-user.service';

describe('InternalUserService', () => {
  const tokenCreateUrl = '/api/User/CreateToken';

  const mockUserCredentials: UserCredentials = {
    username: 'user',
    password: 'user'
  };

  beforeEach(() => TestBed.configureTestingModule({
    imports: [HttpClientTestingModule],
    providers: [{ provide: Router, useValue: null }]
  }));

  it('should be created', () => {
    const service: InternalUserService = TestBed.get(InternalUserService);
    expect(service).toBeTruthy();
  });

  it('should be nologin at first', () => {
    const service: InternalUserService = TestBed.get(InternalUserService);
    expect(service.currentUserInfo).toBe(null);
    service.refreshAndGetUserState().subscribe(result => {
      expect(result).toBe('nologin');
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

    httpController.expectOne((request: HttpRequest<CreateTokenRequest>) =>
      request.url === tokenCreateUrl &&
      request.body.username === 'user' &&
      request.body.password === 'user').flush(<CreateTokenResponse>{
        token: 'test-token',
        userInfo: mockUserInfo
      });

    expect(service.currentUserInfo).toEqual(mockUserInfo);

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

    const tokenValidateRequestMatcher = (req: HttpRequest<ValidateTokenRequest>) => {
      return req.url === '/api/User/ValidateToken' && req.body.token === mockToken;
    };

    beforeEach(() => {
      service = TestBed.get(InternalUserService);
      httpController = TestBed.get(HttpTestingController);

      service.tryLogin(mockUserCredentials).subscribe(); // subscribe to activate login

      httpController.expectOne(tokenCreateUrl).flush(<CreateTokenResponse>{
        token: mockToken,
        userInfo: mockUserInfo
      });
    });

    it('success should work well', () => {
      service.refreshAndGetUserState().subscribe((result: UserLoginState) => {
        expect(result).toEqual(<UserLoginState>'success');
      });

      httpController.expectOne(tokenValidateRequestMatcher).flush(<ValidateTokenResponse>{
        isValid: true,
        userInfo: mockUserInfo
      });

      httpController.verify();
    });

    it('invalid should work well', () => {
      service.refreshAndGetUserState().subscribe((result: UserLoginState) => {
        expect(result).toEqual(<UserLoginState>'invalidlogin');
      });

      httpController.expectOne(tokenValidateRequestMatcher).flush(<ValidateTokenResponse>{ isValid: false });

      httpController.verify();
    });
  });

  // TODO: test on error situations.
});
