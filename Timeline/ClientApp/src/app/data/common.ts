import { AxiosError } from 'axios';

export function extractStatusCode(error: AxiosError): number | null {
  const code = error.response && error.response.status;
  if (typeof code === 'number') {
    return code;
  } else {
    return null;
  }
}

export interface CommonErrorResponse {
  code: number;
  message: string;
}

export function extractErrorCode(error: AxiosError): number | null {
  const { response } = error as AxiosError<CommonErrorResponse>;
  const code = response && response.data && response.data.code;
  if (typeof code === 'number') {
    return code;
  } else {
    return null;
  }
}
