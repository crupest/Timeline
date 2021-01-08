import {
  axios,
  apiBaseUrl,
  convertToNetworkError,
  extractResponseData,
} from "./common";

import {
  HttpTimelineInfo,
  processRawTimelineInfo,
  RawHttpTimelineInfo,
} from "./timeline";

export interface HttpHighlightMoveRequest {
  timeline: string;
  newPosition: number;
}

export interface IHttpHighlightClient {
  list(): Promise<HttpTimelineInfo[]>;
  put(timeline: string): Promise<void>;
  delete(timeline: string): Promise<void>;
  move(req: HttpHighlightMoveRequest): Promise<void>;
}

export class HttpHighlightClient implements IHttpHighlightClient {
  list(): Promise<HttpTimelineInfo[]> {
    return axios
      .get<RawHttpTimelineInfo[]>(`${apiBaseUrl}/highlights`)
      .then(extractResponseData)
      .then((list) => list.map(processRawTimelineInfo))
      .catch(convertToNetworkError);
  }

  put(timeline: string): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/highlights/${timeline}`)
      .catch(convertToNetworkError)
      .then();
  }

  delete(timeline: string): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/highlights/${timeline}`)
      .catch(convertToNetworkError)
      .then();
  }

  move(req: HttpHighlightMoveRequest): Promise<void> {
    return axios
      .post(`${apiBaseUrl}/highlightop/move`, req)
      .catch(convertToNetworkError)
      .then();
  }
}

let client: IHttpHighlightClient = new HttpHighlightClient();

export function getHttpHighlightClient(): IHttpHighlightClient {
  return client;
}

export function setHttpHighlightClient(
  newClient: IHttpHighlightClient
): IHttpHighlightClient {
  const old = client;
  client = newClient;
  return old;
}
