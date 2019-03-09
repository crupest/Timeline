import { UserCredentials, UserInfo } from '../entities';

export type CreateTokenRequest = UserCredentials;

export interface CreateTokenResponse {
  token: string;
  userInfo: UserInfo;
}

export interface ValidateTokenRequest {
  token: string;
}

export interface ValidateTokenResponse {
  isValid: boolean;
  userInfo?: UserInfo;
}
