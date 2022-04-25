// Don't use axios in common because it will contains
// authorization header, which shouldn't be used in token apis.
import originalAxios, { AxiosError } from "axios";

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
      .post<HttpCreateTokenResponse>(`${apiBaseUrl}/v2/token/create`, req, {})
      .then(extractResponseData, (error: AxiosError) => {
        const statusCode = error.response?.status;
        if (statusCode === 422) {
          throw new HttpCreateTokenBadCredentialError(error);
        } else {
          throw error;
        }
      });
  }

  verify(req: HttpVerifyTokenRequest): Promise<HttpVerifyTokenResponse> {
    return axios
      .post<HttpVerifyTokenResponse>(`${apiBaseUrl}/v2/token/verify`, req)
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
