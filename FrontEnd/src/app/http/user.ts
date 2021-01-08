import { AxiosError } from "axios";

import {
  axios,
  apiBaseUrl,
  convertToNetworkError,
  extractResponseData,
  convertToIfStatusCodeIs,
  convertToIfErrorCodeIs,
  NotModified,
  BlobWithEtag,
  convertToBlobWithEtag,
  convertToNotModified,
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
  getAvatar(username: string): Promise<BlobWithEtag>;
  getAvatar(
    username: string,
    etag: string
  ): Promise<BlobWithEtag | NotModified>;
  putAvatar(username: string, data: Blob): Promise<void>;
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
      .then(extractResponseData)
      .catch(convertToNetworkError);
  }

  get(username: string): Promise<HttpUser> {
    return axios
      .get<HttpUser>(`${apiBaseUrl}/users/${username}`)
      .then(extractResponseData)
      .catch(convertToIfStatusCodeIs(404, HttpUserNotExistError))
      .catch(convertToNetworkError);
  }

  patch(username: string, req: HttpUserPatchRequest): Promise<HttpUser> {
    return axios
      .patch<HttpUser>(`${apiBaseUrl}/users/${username}`, req)
      .then(extractResponseData)
      .catch(convertToNetworkError);
  }

  delete(username: string): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/users/${username}`)
      .catch(convertToNetworkError)
      .then();
  }

  getAvatar(username: string): Promise<BlobWithEtag>;
  getAvatar(
    username: string,
    etag?: string
  ): Promise<BlobWithEtag | NotModified> {
    const headers =
      etag != null
        ? {
            "If-None-Match": etag,
          }
        : undefined;

    return axios
      .get(`${apiBaseUrl}/users/${username}/avatar`, {
        responseType: "blob",
        headers,
      })
      .then(convertToBlobWithEtag)
      .catch(convertToNotModified)
      .catch(convertToIfStatusCodeIs(404, HttpUserNotExistError))
      .catch(convertToNetworkError);
  }

  putAvatar(username: string, data: Blob): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/users/${username}/avatar`, data, {
        headers: {
          "Content-Type": data.type,
        },
      })
      .catch(convertToNetworkError)
      .then();
  }

  changePassword(req: HttpChangePasswordRequest): Promise<void> {
    return axios
      .post(`${apiBaseUrl}/userop/changepassword`, req)
      .catch(
        convertToIfErrorCodeIs(11020201, HttpChangePasswordBadCredentialError)
      )
      .catch(convertToNetworkError)
      .then();
  }

  putUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/users/${username}/permissions/${permission}`)
      .catch(convertToNetworkError)
      .then();
  }

  deleteUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/users/${username}/permissions/${permission}`)
      .catch(convertToNetworkError)
      .then();
  }

  createUser(req: HttpCreateUserRequest): Promise<HttpUser> {
    return axios
      .post<HttpUser>(`${apiBaseUrl}/userop/createuser`, req)
      .then(extractResponseData)
      .catch(convertToNetworkError)
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
