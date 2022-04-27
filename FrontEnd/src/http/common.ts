import axios, { Axios, AxiosError, AxiosResponse } from "axios";
import { BehaviorSubject, Observable } from "rxjs";
import { identity } from "lodash";

export { axios };

export const apiBaseUrl = "/api";

export class HttpNetworkError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export class HttpForbiddenError extends Error {
  constructor(
    public type: "unauthorized" | "forbidden",
    public innerError?: AxiosError
  ) {
    super();
  }
}

export class HttpNotFoundError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export class HttpBadRequestError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

function convertNetworkError(error: AxiosError): never {
  if (error.isAxiosError && error.response == null) {
    throw new HttpNetworkError(error);
  } else {
    throw error;
  }
}

function convertForbiddenError(error: AxiosError): never {
  const statusCode = error.response?.status;
  if (statusCode === 401 || statusCode === 403) {
    throw new HttpForbiddenError(
      statusCode === 401 ? "unauthorized" : "forbidden",
      error
    );
  } else {
    throw error;
  }
}

function convertNotFoundError(error: AxiosError): never {
  const statusCode = error.response?.status;
  if (statusCode === 404) {
    throw new HttpNotFoundError(error);
  } else {
    throw error;
  }
}

function convertBadRequestError(error: AxiosError): never {
  const statusCode = error.response?.status;
  if (statusCode === 422) {
    throw new HttpBadRequestError(error);
  } else {
    throw error;
  }
}

export function configureAxios(axios: Axios): void {
  axios.interceptors.response.use(identity, convertNetworkError);
  axios.interceptors.response.use(identity, convertForbiddenError);
  axios.interceptors.response.use(identity, convertNotFoundError);
  axios.interceptors.response.use(identity, convertBadRequestError);
}

configureAxios(axios);

const tokenSubject = new BehaviorSubject<string | null>(null);

export function getHttpToken(): string | null {
  return tokenSubject.value;
}

export function setHttpToken(token: string | null): void {
  tokenSubject.next(token);

  if (token == null) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    delete axios.defaults.headers.common["Authorization"];
  } else {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    axios.defaults.headers.common["Authorization"] = `Bearer ${token}`;
  }
}

export const token$: Observable<string | null> = tokenSubject.asObservable();

export class NotModified {}

export interface BlobWithEtag {
  data: Blob;
  etag: string;
}

export function extractResponseData<T>(res: AxiosResponse<T>): T {
  return res.data;
}

export function convertToNotModified(error: AxiosError): NotModified {
  const statusCode = error.response?.status;
  if (statusCode == 304) {
    return new NotModified();
  } else {
    throw error;
  }
}

export function convertToBlobWithEtag(res: AxiosResponse<Blob>): BlobWithEtag {
  return {
    data: res.data,
    etag: (res.headers as Record<"etag", string>)["etag"],
  };
}

export function extractEtag(res: AxiosResponse): string {
  return (res.headers as Record<"etag", string>)["etag"];
}

export interface Page<T> {
  pageNumber: number;
  pageSize: number;
  totalPageCount: number;
  totalCount: number;
  items: T[];
}
