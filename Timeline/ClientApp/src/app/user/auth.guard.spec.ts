import { Observable, of } from 'rxjs';

import { AuthGuard, AuthStrategy } from './auth.guard';
import { UserInfo } from './entities';

describe('AuthGuard', () => {
  class ConfiurableAuthGuard extends AuthGuard {
    constructor(mockInternalUserService: any) {
      super(mockInternalUserService);
    }

    authStrategy: AuthStrategy = 'all';
  }

  let mockUserService: { userInfo$: Observable<UserInfo | null> };
  let guard: ConfiurableAuthGuard;
  let onAuthFialedSpy: jasmine.Spy;

  const mockRoles = ['role1', 'role2'];

  interface ActivateResultMap {
    nologin: boolean;
    loginWithNoRole: boolean;
    loginWithMockRoles: boolean;
  }


  function createTest(authStrategy: AuthStrategy, result: ActivateResultMap): () => void {
    return () => {
      guard.authStrategy = authStrategy;

      function testWith(userInfo: UserInfo | null, r: boolean) {
        mockUserService.userInfo$ = of(userInfo);

        const rawResult = guard.canActivate(<any>null, <any>null);
        if (typeof rawResult === 'boolean') {
          expect(rawResult).toBe(r);
        } else if (rawResult instanceof Observable) {
          rawResult.subscribe(next => expect(next).toBe(r));
        } else {
          throw new Error('Unsupported return type.');
        }
      }

      testWith(null, result.nologin);
      testWith({ username: 'user', roles: [] }, result.loginWithNoRole);
      testWith({ username: 'user', roles: mockRoles }, result.loginWithMockRoles);
    };
  }

  beforeEach(() => {
    mockUserService = { userInfo$: of(null) };
    guard = new ConfiurableAuthGuard(mockUserService);
    onAuthFialedSpy = spyOn(guard, 'onAuthFailed');
  });


  it('all should work', createTest('all', { nologin: true, loginWithNoRole: true, loginWithMockRoles: true }));
  it('require login should work', createTest('requirelogin', { nologin: false, loginWithNoRole: true, loginWithMockRoles: true }));
  it('require no login should work', createTest('requirenologin', { nologin: true, loginWithNoRole: false, loginWithMockRoles: false }));
  it('good roles should work', createTest(mockRoles, { nologin: false, loginWithNoRole: false, loginWithMockRoles: true }));
  it('bad roles should work', createTest(['role3'], { nologin: false, loginWithNoRole: false, loginWithMockRoles: false }));

  it('auth failed callback should be called', () => {
    guard.authStrategy = 'requirelogin';
    (<Observable<boolean>>guard.canActivate(<any>null, <any>null)).subscribe();
    expect(onAuthFialedSpy).toHaveBeenCalled();
  });
});
