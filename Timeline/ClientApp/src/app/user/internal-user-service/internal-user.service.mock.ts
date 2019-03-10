import { InternalUserService } from './internal-user.service';

export function createMockInternalUserService(): jasmine.SpyObj<InternalUserService> {
  return jasmine.createSpyObj('InternalUserService', ['userRouteNavigate', 'refreshAndGetUserState', 'tryLogin']);
}
