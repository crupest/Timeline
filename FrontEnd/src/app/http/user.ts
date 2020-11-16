import axios, { AxiosError } from "axios";

import {
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

export const kUserPermissionList = [
  "UserManagement",
  "AllTimelineManagement",
  "HighlightTimelineManagement",
] as const;

export type UserPermission = typeof kUserPermissionList[number];

export interface HttpUser {
  uniqueId: string;
  username: string;
  permissions: UserPermission[];
  nickname: string;
}

export interface HttpUserPatchRequest {
  nickname?: string;
}

export interface HttpChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
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
  patch(
    username: string,
    req: HttpUserPatchRequest,
    token: string
  ): Promise<HttpUser>;
  getAvatar(username: string): Promise<BlobWithEtag>;
  getAvatar(
    username: string,
    etag: string
  ): Promise<BlobWithEtag | NotModified>;
  putAvatar(username: string, data: Blob, token: string): Promise<void>;
  changePassword(req: HttpChangePasswordRequest, token: string): Promise<void>;
  putUserPermission(
    username: string,
    permission: UserPermission,
    token: string
  ): Promise<void>;
  deleteUserPermission(
    username: string,
    permission: UserPermission,
    token: string
  ): Promise<void>;
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

  patch(
    username: string,
    req: HttpUserPatchRequest,
    token: string
  ): Promise<HttpUser> {
    return axios
      .patch<HttpUser>(`${apiBaseUrl}/users/${username}?token=${token}`, req)
      .then(extractResponseData)
      .catch(convertToNetworkError);
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

  putAvatar(username: string, data: Blob, token: string): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/users/${username}/avatar?token=${token}`, data, {
        headers: {
          "Content-Type": data.type,
        },
      })
      .catch(convertToNetworkError)
      .then();
  }

  changePassword(req: HttpChangePasswordRequest, token: string): Promise<void> {
    return axios
      .post(`${apiBaseUrl}/userop/changepassword?token=${token}`, req)
      .catch(
        convertToIfErrorCodeIs(11020201, HttpChangePasswordBadCredentialError)
      )
      .catch(convertToNetworkError)
      .then();
  }

  putUserPermission(
    username: string,
    permission: UserPermission,
    token: string
  ): Promise<void> {
    return axios
      .put(
        `${apiBaseUrl}/users/${username}/permissions/${permission}?token=${token}`
      )
      .catch(convertToNetworkError)
      .then();
  }

  deleteUserPermission(
    username: string,
    permission: UserPermission,
    token: string
  ): Promise<void> {
    return axios
      .delete(
        `${apiBaseUrl}/users/${username}/permissions/${permission}?token=${token}`
      )
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
