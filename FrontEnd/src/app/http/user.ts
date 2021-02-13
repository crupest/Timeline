import { AxiosError } from "axios";

import {
  axios,
  apiBaseUrl,
  extractResponseData,
  convertToIfStatusCodeIs,
  convertToIfErrorCodeIs,
  extractEtag,
} from "./common";

export const kUserManagement = "UserManagement";
export const kAllTimelineManagement = "AllTimelineManagement";
export const kHighlightTimelineManagement = "HighlightTimelineManagement";

export const kUserPermissionList = [
  kUserManagement,
  kAllTimelineManagement,
  kHighlightTimelineManagement,
] as const;

export type UserPermission = typeof kUserPermissionList[number];

export interface HttpUser {
  uniqueId: string;
  username: string;
  permissions: UserPermission[];
  nickname: string;
}

export interface HttpUserPatchRequest {
  username?: string;
  password?: string;
  nickname?: string;
}

export interface HttpChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface HttpCreateUserRequest {
  username: string;
  password: string;
}

export class HttpUserNotExistError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export class HttpChangePasswordBadCredentialError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export interface IHttpUserClient {
  list(): Promise<HttpUser[]>;
  get(username: string): Promise<HttpUser>;
  patch(username: string, req: HttpUserPatchRequest): Promise<HttpUser>;
  delete(username: string): Promise<void>;
  // return etag
  putAvatar(username: string, data: Blob): Promise<string>;
  changePassword(req: HttpChangePasswordRequest): Promise<void>;
  putUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void>;
  deleteUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void>;

  createUser(req: HttpCreateUserRequest, token: string): Promise<HttpUser>;
}

export class HttpUserClient implements IHttpUserClient {
  list(): Promise<HttpUser[]> {
    return axios
      .get<HttpUser[]>(`${apiBaseUrl}/users`)
      .then(extractResponseData);
  }

  get(username: string): Promise<HttpUser> {
    return axios
      .get<HttpUser>(`${apiBaseUrl}/users/${username}`)
      .then(extractResponseData)
      .catch(convertToIfStatusCodeIs(404, HttpUserNotExistError));
  }

  patch(username: string, req: HttpUserPatchRequest): Promise<HttpUser> {
    return axios
      .patch<HttpUser>(`${apiBaseUrl}/users/${username}`, req)
      .then(extractResponseData);
  }

  delete(username: string): Promise<void> {
    return axios.delete(`${apiBaseUrl}/users/${username}`).then();
  }

  putAvatar(username: string, data: Blob): Promise<string> {
    return axios
      .put(`${apiBaseUrl}/users/${username}/avatar`, data, {
        headers: {
          "Content-Type": data.type,
        },
      })
      .then(extractEtag);
  }

  changePassword(req: HttpChangePasswordRequest): Promise<void> {
    return axios
      .post(`${apiBaseUrl}/userop/changepassword`, req)
      .catch(
        convertToIfErrorCodeIs(11020201, HttpChangePasswordBadCredentialError)
      )
      .then();
  }

  putUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/users/${username}/permissions/${permission}`)
      .then();
  }

  deleteUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/users/${username}/permissions/${permission}`)
      .then();
  }

  createUser(req: HttpCreateUserRequest): Promise<HttpUser> {
    return axios
      .post<HttpUser>(`${apiBaseUrl}/userop/createuser`, req)
      .then(extractResponseData)
      .then();
  }
}

let client: IHttpUserClient = new HttpUserClient();

export function getHttpUserClient(): IHttpUserClient {
  return client;
}

export function setHttpUserClient(newClient: IHttpUserClient): IHttpUserClient {
  const old = client;
  client = newClient;
  return old;
}
