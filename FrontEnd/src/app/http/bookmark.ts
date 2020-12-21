import axios from "axios";

import {
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

export interface IHttpBookmarkClient {
  list(token: string): Promise<HttpTimelineInfo[]>;
  put(timeline: string, token: string): Promise<void>;
  delete(timeline: string, token: string): Promise<void>;
  move(req: HttpHighlightMoveRequest, token: string): Promise<void>;
}

export class HttpHighlightClient implements IHttpBookmarkClient {
  list(token: string): Promise<HttpTimelineInfo[]> {
    return axios
      .get<RawHttpTimelineInfo[]>(`${apiBaseUrl}/bookmarks?token=${token}`)
      .then(extractResponseData)
      .then((list) => list.map(processRawTimelineInfo))
      .catch(convertToNetworkError);
  }

  put(timeline: string, token: string): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/bookmarks/${timeline}?token=${token}`)
      .catch(convertToNetworkError)
      .then();
  }

  delete(timeline: string, token: string): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/bookmarks/${timeline}?token=${token}`)
      .catch(convertToNetworkError)
      .then();
  }

  move(req: HttpHighlightMoveRequest, token: string): Promise<void> {
    return axios
      .post(`${apiBaseUrl}/bookmarkop/move?token=${token}`, req)
      .catch(convertToNetworkError)
      .then();
  }
}

let client: IHttpBookmarkClient = new HttpHighlightClient();

export function getHttpBookmarkClient(): IHttpBookmarkClient {
  return client;
}

export function setHttpBookmarkClient(
  newClient: IHttpBookmarkClient
): IHttpBookmarkClient {
  const old = client;
  client = newClient;
  return old;
}
