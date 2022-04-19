import axios, { Axios, AxiosError, AxiosResponse } from "axios";
import { Base64 } from "js-base64";
import { identity } from "lodash";
import { BehaviorSubject, Observable } from "rxjs";

export { axios };

export const apiBaseUrl = "/api";

export class HttpNetworkError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export class HttpForbiddenError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export class HttpNotFoundError extends Error {
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
  if (
    error.isAxiosError &&
    error.response != null &&
    (error.response.status == 401 || error.response.status == 403)
  ) {
    throw new HttpForbiddenError(error);
  } else {
    throw error;
  }
}

function convertNotFoundError(error: AxiosError): never {
  if (
    error.isAxiosError &&
    error.response != null &&
    error.response.status == 404
  ) {
    throw new HttpNotFoundError(error);
  } else {
    throw error;
  }
}

export function configureAxios(axios: Axios): void {
  axios.interceptors.response.use(identity, convertNetworkError);
  axios.interceptors.response.use(identity, convertForbiddenError);
  axios.interceptors.response.use(identity, convertNotFoundError);
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

export function base64(blob: Blob | string): Promise<string> {
  if (typeof blob === "string") {
    return Promise.resolve(Base64.encode(blob));
  }

  return new Promise<string>((resolve) => {
    const reader = new FileReader();
    reader.onload = function () {
      resolve((reader.result as string).replace(/^data:.*;base64,/, ""));
    };
    reader.readAsDataURL(blob);
  });
}

export function extractStatusCode(error: AxiosError): number | null {
  if (error.isAxiosError) {
    const code = error?.response?.status;
    if (typeof code === "number") {
      return code;
    }
  }
  return null;
}

export interface CommonErrorResponse {
  code: number;
  message: string;
}

export function extractErrorCode(
  error: AxiosError<CommonErrorResponse>
): number | null {
  if (error.isAxiosError) {
    const code = error.response?.data?.code;
    if (typeof code === "number") {
      return code;
    }
  }
  return null;
}

export class NotModified {}

export interface BlobWithEtag {
  data: Blob;
  etag: string;
}

export function extractResponseData<T>(res: AxiosResponse<T>): T {
  return res.data;
}

export function catchIfStatusCodeIs<
  TResult,
  TErrorHandlerResult extends TResult | PromiseLike<TResult> | null | undefined
>(
  statusCode: number,
  errorHandler: (error: AxiosError<CommonErrorResponse>) => TErrorHandlerResult
): (error: AxiosError<CommonErrorResponse>) => TErrorHandlerResult {
  return (error: AxiosError<CommonErrorResponse>) => {
    if (extractStatusCode(error) == statusCode) {
      return errorHandler(error);
    } else {
      throw error;
    }
  };
}

export function convertToIfStatusCodeIs<NewError>(
  statusCode: number,
  newErrorType: {
    new (innerError: AxiosError): NewError;
  }
): (error: AxiosError<CommonErrorResponse>) => never {
  return catchIfStatusCodeIs(statusCode, (error) => {
    throw new newErrorType(error);
  });
}

export function catchIfErrorCodeIs<
  TResult,
  TErrorHandlerResult extends TResult | PromiseLike<TResult> | null | undefined
>(
  errorCode: number,
  errorHandler: (error: AxiosError<CommonErrorResponse>) => TErrorHandlerResult
): (error: AxiosError<CommonErrorResponse>) => TErrorHandlerResult {
  return (error: AxiosError<CommonErrorResponse>) => {
    if (extractErrorCode(error) == errorCode) {
      return errorHandler(error);
    } else {
      throw error;
    }
  };
}
export function convertToIfErrorCodeIs<NewError>(
  errorCode: number,
  newErrorType: {
    new (innerError: AxiosError): NewError;
  }
): (error: AxiosError<CommonErrorResponse>) => never {
  return catchIfErrorCodeIs(errorCode, (error) => {
    throw new newErrorType(error);
  });
}

export function convertToNotModified(
  error: AxiosError<CommonErrorResponse>
): NotModified {
  if (
    error.isAxiosError &&
    error.response != null &&
    error.response.status == 304
  ) {
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
