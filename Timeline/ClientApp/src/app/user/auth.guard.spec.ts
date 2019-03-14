import { AuthGuard, AuthStrategy } from './auth.guard';
import { UserInfo } from './entities';

describe('AuthGuard', () => {
  class ConfiurableAuthGuard extends AuthGuard {
    constructor(mockInternalUserService: any) {
      super(mockInternalUserService);
    }

    authStrategy: AuthStrategy = 'all';
    onAuthFailed: () => void = () => { };
  }

  let mockUserService: { currentUserInfo: UserInfo | null };
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

      mockUserService.currentUserInfo = null;
      expect(guard.canActivate(<any>null, <any>null)).toBe(result.nologin);

      mockUserService.currentUserInfo = { username: 'user', roles: [] };
      expect(guard.canActivate(<any>null, <any>null)).toBe(result.loginWithNoRole);

      mockUserService.currentUserInfo = { username: 'user', roles: mockRoles };
      expect(guard.canActivate(<any>null, <any>null)).toBe(result.loginWithMockRoles);
    };
  }

  beforeEach(() => {
    mockUserService = { currentUserInfo: null };
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
    mockUserService.currentUserInfo = null;
    guard.canActivate(<any>null, <any>null);
    expect(onAuthFialedSpy).toHaveBeenCalled();
  });
});
