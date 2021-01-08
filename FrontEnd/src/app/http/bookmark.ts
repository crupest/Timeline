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

export interface IHttpBookmarkClient {
  list(): Promise<HttpTimelineInfo[]>;
  put(timeline: string): Promise<void>;
  delete(timeline: string): Promise<void>;
  move(req: HttpHighlightMoveRequest): Promise<void>;
}

export class HttpHighlightClient implements IHttpBookmarkClient {
  list(): Promise<HttpTimelineInfo[]> {
    return axios
      .get<RawHttpTimelineInfo[]>(`${apiBaseUrl}/bookmarks`)
      .then(extractResponseData)
      .then((list) => list.map(processRawTimelineInfo))
      .catch(convertToNetworkError);
  }

  put(timeline: string): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/bookmarks/${timeline}`)
      .catch(convertToNetworkError)
      .then();
  }

  delete(timeline: string): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/bookmarks/${timeline}`)
      .catch(convertToNetworkError)
      .then();
  }

  move(req: HttpHighlightMoveRequest): Promise<void> {
    return axios
      .post(`${apiBaseUrl}/bookmarkop/move`, req)
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
