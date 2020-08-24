import { AxiosError, AxiosResponse } from "axios";

export const apiBaseUrl = "/api";

export function base64(blob: Blob): Promise<string> {
  return new Promise<string>((resolve) => {
    const reader = new FileReader();
    reader.onload = function () {
      resolve((reader.result as string).replace(/^data:.+;base64,/, ""));
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

export function convertToNetworkError(
  error: AxiosError<CommonErrorResponse>
): never {
  if (error.isAxiosError && error.response == null) {
    throw new HttpNetworkError(error);
  } else {
    throw error;
  }
}

export function convertToForbiddenError(
  error: AxiosError<CommonErrorResponse>
): never {
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
