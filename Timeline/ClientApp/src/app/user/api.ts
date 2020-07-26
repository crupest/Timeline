import { getHttpUserClient } from '../http/user';
import { User } from '../data/user';

export function changeNickname(
  token: string,
  username: string,
  newNickname: string
): Promise<User> {
  return getHttpUserClient().patch(username, { nickname: newNickname }, token);
}
