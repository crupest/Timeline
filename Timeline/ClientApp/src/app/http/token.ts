import axios, { AxiosError } from 'axios';

import {
  apiBaseUrl,
  convertToNetworkError,
  convertToIfErrorCodeIs,
  extractResponseData,
} from './common';
import { HttpUser } from './user';

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

export class HttpCreateTokenBadCredentialError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export interface IHttpTokenClient {
  create(req: HttpCreateTokenRequest): Promise<HttpCreateTokenResponse>;
  verify(req: HttpVerifyTokenRequest): Promise<HttpVerifyTokenResponse>;
}

export class HttpTokenClient implements IHttpTokenClient {
  create(req: HttpCreateTokenRequest): Promise<HttpCreateTokenResponse> {
    return axios
      .post<HttpCreateTokenResponse>(`${apiBaseUrl}/token/create`, req)
      .then(extractResponseData)
      .catch(
        convertToIfErrorCodeIs(11010101, HttpCreateTokenBadCredentialError)
      )
      .catch(convertToNetworkError);
  }

  verify(req: HttpVerifyTokenRequest): Promise<HttpVerifyTokenResponse> {
    return axios
      .post<HttpVerifyTokenResponse>(`${apiBaseUrl}/token/verify`, req)
      .then(extractResponseData)
      .catch(convertToNetworkError);
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
