import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpRequest } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController, TestRequest } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material';

import { Mock } from 'src/app/test-utilities/mock';
import { createMockStorage } from 'src/app/test-utilities/storage.mock';
import { WINDOW } from '../window-inject-token';

import { UserInfo, UserCredentials } from '../entities';
import {
  createTokenUrl, validateTokenUrl, CreateTokenRequest,
  CreateTokenResponse, ValidateTokenRequest, ValidateTokenResponse
} from './http-entities';
import { InternalUserService, SnackBarTextKey, snackBarText, TOKEN_STORAGE_KEY } from './internal-user.service';
import { repeat } from 'src/app/utilities/language-untilities';


describe('InternalUserService', () => {
  let mockLocalStorage: Mock<Storage>;
  let mockSnackBar: jasmine.SpyObj<MatSnackBar>;

  beforeEach(() => {
    mockLocalStorage = createMockStorage();
    mockSnackBar = jasmine.createSpyObj('MatSnackBar', ['open']);
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: WINDOW, useValue: { localStorage: mockLocalStorage } },
        { provide: Router, useValue: null },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ]
    });
  });

  it('should be created', () => {
    const service: InternalUserService = TestBed.get(InternalUserService);
    expect(service).toBeTruthy();
  });

  const mockUserInfo: UserInfo = {
    username: 'user',
    roles: ['user', 'other']
  };

  const mockToken = 'mock-token';

  describe('validate token', () => {
    const validateTokenRequestMatcher = (req: HttpRequest<ValidateTokenRequest>): boolean =>
      req.url === validateTokenUrl && req.body !== null && req.body.token === mockToken;

    function createTest(
      expectSnackBarTextKey: SnackBarTextKey,
      setStorageToken: boolean,
      setHttpController?: (controller: HttpTestingController) => void
    ): () => void {
      return fakeAsync(() => {
        if (setStorageToken) {
          mockLocalStorage.setItem(TOKEN_STORAGE_KEY, mockToken);
        }
        TestBed.get(InternalUserService);
        const controller = TestBed.get(HttpTestingController) as HttpTestingController;
        if (setHttpController) {
          setHttpController(controller);
        }
        controller.verify();
        tick();
        expect(mockSnackBar.open).toHaveBeenCalledWith(snackBarText[expectSnackBarTextKey], jasmine.anything(), jasmine.anything());
      });
    }

    it('no login should work well', createTest('noLogin', false));
    it('already login should work well', createTest('alreadyLogin', true,
      controller => controller.expectOne(validateTokenRequestMatcher).flush(
        <ValidateTokenResponse>{ isValid: true, userInfo: mockUserInfo })));
    it('invalid login should work well', createTest('invalidLogin', true,
      controller => controller.expectOne(validateTokenRequestMatcher).flush(<ValidateTokenResponse>{ isValid: false })));
    it('check fail should work well', createTest('checkFail', true,
      controller => repeat(4, () => {
        controller.expectOne(validateTokenRequestMatcher).error(new ErrorEvent('Network error', { message: 'simulated network error' }));
      })));
  });

  describe('login should work well', () => {
    const mockUserCredentials: UserCredentials = {
      username: 'user',
      password: 'user'
    };

    function createTest(rememberMe: boolean) {
      return () => {
        const service: InternalUserService = TestBed.get(InternalUserService);

        service.tryLogin({ ...mockUserCredentials, rememberMe: rememberMe }).subscribe(result => {
          expect(result).toEqual(mockUserInfo);
        });

        const httpController = TestBed.get(HttpTestingController) as HttpTestingController;

        httpController.expectOne((request: HttpRequest<CreateTokenRequest>) =>
          request.url === createTokenUrl && request.body !== null &&
          request.body.username === mockUserCredentials.username &&
          request.body.password === mockUserCredentials.password).flush(<CreateTokenResponse>{
            token: mockToken,
            userInfo: mockUserInfo
          });

        expect(service.currentUserInfo).toEqual(mockUserInfo);

        httpController.verify();

        expect(mockLocalStorage.getItem(TOKEN_STORAGE_KEY)).toBe(rememberMe ? mockToken : null);
      }
    }

    it('remember me should work well', createTest(true));
    it('not remember me should work well', createTest(false));
  });

  // TODO: test on error situations.
});
