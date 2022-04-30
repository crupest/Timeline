import { axios, apiBaseUrl, extractResponseData, extractEtag } from "./common";

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

export interface HttpBookmarkVisibility {
  visibility: "Private" | "Register" | "Public";
}

export interface HttpRegisterCode {
  registerCode?: string;
}

export interface IHttpUserClient {
  list(): Promise<HttpUser[]>;
  get(username: string): Promise<HttpUser>;
  post(req: HttpCreateUserRequest): Promise<HttpUser>;
  patch(username: string, req: HttpUserPatchRequest): Promise<HttpUser>;
  delete(username: string): Promise<void>;
  generateAvatarUrl(username: string): string;
  putAvatar(username: string, data: Blob): Promise<string>;
  putUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void>;
  deleteUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void>;
  changePassword(req: HttpChangePasswordRequest): Promise<void>;

  getBookmarkVisibility(username: string): Promise<HttpBookmarkVisibility>;
  putBookmarkVisibility(
    username: string,
    req: HttpBookmarkVisibility
  ): Promise<void>;

  getRegisterCode(username: string): Promise<HttpRegisterCode>;
  renewRegisterCode(username: string): Promise<void>;
}

export class HttpUserClient implements IHttpUserClient {
  list(): Promise<HttpUser[]> {
    return axios
      .get<HttpUser[]>(`${apiBaseUrl}/v2/users`)
      .then(extractResponseData);
  }

  get(username: string): Promise<HttpUser> {
    return axios
      .get<HttpUser>(`${apiBaseUrl}/v2/users/${username}`)
      .then(extractResponseData);
  }

  post(req: HttpCreateUserRequest): Promise<HttpUser> {
    return axios
      .post<HttpUser>(`${apiBaseUrl}/v2/users`, req)
      .then(extractResponseData)
      .then();
  }

  patch(username: string, req: HttpUserPatchRequest): Promise<HttpUser> {
    return axios
      .patch<HttpUser>(`${apiBaseUrl}/v2/users/${username}`, req)
      .then(extractResponseData);
  }

  delete(username: string): Promise<void> {
    return axios.delete(`${apiBaseUrl}/v2/users/${username}`).then();
  }

  generateAvatarUrl(username: string): string {
    return `${apiBaseUrl}/v2/users/${username}/avatar`;
  }

  putAvatar(username: string, data: Blob): Promise<string> {
    return axios
      .put(`${apiBaseUrl}/v2/users/${username}/avatar`, data, {
        headers: {
          "Content-Type": data.type,
        },
      })
      .then(extractEtag);
  }

  putUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/v2/users/${username}/permissions/${permission}`)
      .then();
  }

  deleteUserPermission(
    username: string,
    permission: UserPermission
  ): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/v2/users/${username}/permissions/${permission}`)
      .then();
  }

  changePassword(req: HttpChangePasswordRequest): Promise<void> {
    return axios.post(`${apiBaseUrl}/v2/self/changepassword`, req).then();
  }

  getBookmarkVisibility(username: string): Promise<HttpBookmarkVisibility> {
    return axios
      .get<HttpBookmarkVisibility>(
        `${apiBaseUrl}/v2/users/${username}/bookmarks/visibility`
      )
      .then(extractResponseData);
  }

  putBookmarkVisibility(
    username: string,
    req: HttpBookmarkVisibility
  ): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/v2/users/${username}/bookmarks/visibility`, req)
      .then();
  }

  getRegisterCode(username: string): Promise<HttpRegisterCode> {
    return axios
      .get<HttpRegisterCode>(`${apiBaseUrl}/v2/users/${username}/registercode`)
      .then(extractResponseData);
  }

  renewRegisterCode(username: string): Promise<void> {
    return axios
      .post(`${apiBaseUrl}/v2/users/${username}/renewregistercode`)
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
