import { AxiosError } from "axios";

import { applyQueryParameters } from "../utilities/url";

import {
  axios,
  apiBaseUrl,
  extractResponseData,
  convertToIfErrorCodeIs,
  getHttpToken,
} from "./common";
import { HttpUser } from "./user";

export const kTimelineVisibilities = ["Public", "Register", "Private"] as const;

export type TimelineVisibility = typeof kTimelineVisibilities[number];

export interface HttpTimelineInfo {
  uniqueId: string;
  title: string;
  name: string;
  description: string;
  owner: HttpUser;
  visibility: TimelineVisibility;
  color: string;
  lastModified: string;
  members: HttpUser[];
  isHighlight: boolean;
  isBookmark: boolean;
  manageable: boolean;
  postable: boolean;
}

export interface HttpTimelineListQuery {
  visibility?: TimelineVisibility;
  relate?: string;
  relateType?: "own" | "join";
}

export interface HttpTimelinePostRequest {
  name: string;
}

export interface HttpTimelinePostDataDigest {
  kind: string;
  eTag: string;
  lastUpdated: string;
}

export interface HttpTimelinePostInfo {
  id: number;
  time: string;
  author: HttpUser;
  dataList: HttpTimelinePostDataDigest[];
  color: string;
  lastUpdated: string;
  timelineName: string;
  editable: boolean;
}

export interface HttpTimelinePostPostRequestData {
  contentType: string;
  data: string;
}

export interface HttpTimelinePostPostRequest {
  time?: string;
  color?: string;
  dataList: HttpTimelinePostPostRequestData[];
}

export interface HttpTimelinePatchRequest {
  name?: string;
  title?: string;
  visibility?: TimelineVisibility;
  description?: string;
}

export class HttpTimelineNameConflictError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export interface IHttpTimelineClient {
  listTimeline(query: HttpTimelineListQuery): Promise<HttpTimelineInfo[]>;
  getTimeline(timelineName: string): Promise<HttpTimelineInfo>;
  postTimeline(req: HttpTimelinePostRequest): Promise<HttpTimelineInfo>;
  patchTimeline(
    timelineName: string,
    req: HttpTimelinePatchRequest
  ): Promise<HttpTimelineInfo>;
  deleteTimeline(timelineName: string): Promise<void>;
  memberPut(timelineName: string, username: string): Promise<void>;
  memberDelete(timelineName: string, username: string): Promise<void>;
  listPost(timelineName: string): Promise<HttpTimelinePostInfo[]>;
  generatePostDataUrl(timelineName: string, postId: number): string;
  getPostDataAsString(timelineName: string, postId: number): Promise<string>;
  postPost(
    timelineName: string,
    req: HttpTimelinePostPostRequest
  ): Promise<HttpTimelinePostInfo>;
  deletePost(timelineName: string, postId: number): Promise<void>;
}

export class HttpTimelineClient implements IHttpTimelineClient {
  listTimeline(query: HttpTimelineListQuery): Promise<HttpTimelineInfo[]> {
    return axios
      .get<HttpTimelineInfo[]>(
        applyQueryParameters(`${apiBaseUrl}/timelines`, query)
      )
      .then(extractResponseData);
  }

  getTimeline(timelineName: string): Promise<HttpTimelineInfo> {
    return axios
      .get<HttpTimelineInfo>(`${apiBaseUrl}/timelines/${timelineName}`)
      .then(extractResponseData);
  }

  postTimeline(req: HttpTimelinePostRequest): Promise<HttpTimelineInfo> {
    return axios
      .post<HttpTimelineInfo>(`${apiBaseUrl}/timelines`, req)
      .then(extractResponseData)
      .catch(convertToIfErrorCodeIs(11040101, HttpTimelineNameConflictError));
  }

  patchTimeline(
    timelineName: string,
    req: HttpTimelinePatchRequest
  ): Promise<HttpTimelineInfo> {
    return axios
      .patch<HttpTimelineInfo>(`${apiBaseUrl}/timelines/${timelineName}`, req)
      .then(extractResponseData);
  }

  deleteTimeline(timelineName: string): Promise<void> {
    return axios.delete(`${apiBaseUrl}/timelines/${timelineName}`).then();
  }

  memberPut(timelineName: string, username: string): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/timelines/${timelineName}/members/${username}`)
      .then();
  }

  memberDelete(timelineName: string, username: string): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/timelines/${timelineName}/members/${username}`)
      .then();
  }

  listPost(timelineName: string): Promise<HttpTimelinePostInfo[]> {
    return axios
      .get<HttpTimelinePostInfo[]>(
        `${apiBaseUrl}/timelines/${timelineName}/posts`
      )
      .then(extractResponseData);
  }

  generatePostDataUrl(timelineName: string, postId: number): string {
    return applyQueryParameters(
      `${apiBaseUrl}/timelines/${timelineName}/posts/${postId}/data`,
      { token: getHttpToken() }
    );
  }

  getPostDataAsString(timelineName: string, postId: number): Promise<string> {
    return axios
      .get<string>(
        `${apiBaseUrl}/timelines/${timelineName}/posts/${postId}/data`,
        {
          responseType: "text",
        }
      )
      .then(extractResponseData);
  }

  postPost(
    timelineName: string,
    req: HttpTimelinePostPostRequest
  ): Promise<HttpTimelinePostInfo> {
    return axios
      .post<HttpTimelinePostInfo>(
        `${apiBaseUrl}/timelines/${timelineName}/posts`,
        req
      )
      .then(extractResponseData);
  }

  deletePost(timelineName: string, postId: number): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/timelines/${timelineName}/posts/${postId}`)
      .then();
  }
}

let client: IHttpTimelineClient = new HttpTimelineClient();

export function getHttpTimelineClient(): IHttpTimelineClient {
  return client;
}

export function setHttpTimelineClient(
  newClient: IHttpTimelineClient
): IHttpTimelineClient {
  const old = client;
  client = newClient;
  return old;
}
