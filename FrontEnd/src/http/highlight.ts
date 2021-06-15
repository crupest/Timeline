import { axios, apiBaseUrl, extractResponseData } from "./common";

import { HttpTimelineInfo } from "./timeline";

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
      .get<HttpTimelineInfo[]>(`${apiBaseUrl}/highlights`)
      .then(extractResponseData);
  }

  put(timeline: string): Promise<void> {
    return axios.put(`${apiBaseUrl}/highlights/${timeline}`).then();
  }

  delete(timeline: string): Promise<void> {
    return axios.delete(`${apiBaseUrl}/highlights/${timeline}`).then();
  }

  move(req: HttpHighlightMoveRequest): Promise<void> {
    return axios.post(`${apiBaseUrl}/highlightop/move`, req).then();
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
