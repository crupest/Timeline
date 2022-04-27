// Don't use axios in common because it will contains
// authorization header, which shouldn't be used in token apis.
import originalAxios from "axios";

import { apiBaseUrl, extractResponseData, configureAxios } from "./common";

import { HttpUser } from "./user";

const axios = originalAxios.create();
configureAxios(axios);

export interface HttpCreateTokenRequest {
  username: string;
  password: string;
  expire: number;
}

export interface HttpCreateTokenResponse {
  token: string;
  user: HttpUser;
}

export interface HttpVerifyTokenRequest {
  token: string;
}

export interface HttpVerifyTokenResponse {
  user: HttpUser;
}

export interface HttpRegisterRequest {
  username: string;
  password: string;
  nickname?: string;
  registerCode: string;
}

export interface IHttpTokenClient {
  create(req: HttpCreateTokenRequest): Promise<HttpCreateTokenResponse>;
  verify(req: HttpVerifyTokenRequest): Promise<HttpVerifyTokenResponse>;
  register(req: HttpRegisterRequest): Promise<HttpUser>;
}

export class HttpTokenClient implements IHttpTokenClient {
  create(req: HttpCreateTokenRequest): Promise<HttpCreateTokenResponse> {
    return axios
      .post<HttpCreateTokenResponse>(`${apiBaseUrl}/v2/token/create`, req, {})
      .then(extractResponseData);
  }

  verify(req: HttpVerifyTokenRequest): Promise<HttpVerifyTokenResponse> {
    return axios
      .post<HttpVerifyTokenResponse>(`${apiBaseUrl}/v2/token/verify`, req)
      .then(extractResponseData);
  }

  register(req: HttpRegisterRequest): Promise<HttpUser> {
    return axios
      .post<HttpUser>(`${apiBaseUrl}/v2/register`, req)
      .then(extractResponseData);
  }
}

let client: IHttpTokenClient = new HttpTokenClient();

export function getHttpTokenClient(): IHttpTokenClient {
  return client;
}

export function setHttpTokenClient(
  newClient: IHttpTokenClient
): IHttpTokenClient {
  const old = client;
  client = newClient;
  return old;
}
