import { axios, apiBaseUrl, extractResponseData } from "./common";

import { HttpTimelineInfo } from "./timeline";

export interface HttpHighlightMoveRequest {
  timeline: string;
  newPosition: number;
}

export interface IHttpBookmarkClient {
  list(): Promise<HttpTimelineInfo[]>;
  put(timeline: string): Promise<void>;
  delete(timeline: string): Promise<void>;
  move(req: HttpHighlightMoveRequest): Promise<void>;
}

export class HttpHighlightClient implements IHttpBookmarkClient {
  list(): Promise<HttpTimelineInfo[]> {
    return axios
      .get<HttpTimelineInfo[]>(`${apiBaseUrl}/bookmarks`)
      .then(extractResponseData);
  }

  put(timeline: string): Promise<void> {
    return axios.put(`${apiBaseUrl}/bookmarks/${timeline}`).then();
  }

  delete(timeline: string): Promise<void> {
    return axios.delete(`${apiBaseUrl}/bookmarks/${timeline}`).then();
  }

  move(req: HttpHighlightMoveRequest): Promise<void> {
    return axios.post(`${apiBaseUrl}/bookmarkop/move`, req).then();
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
