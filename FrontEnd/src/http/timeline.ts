import { withQuery } from "@/utilities/url";

import {
  axios,
  apiBaseUrl,
  extractResponseData,
  getHttpToken,
  Page,
} from "./common";
import { HttpUser } from "./user";

export const kTimelineVisibilities = ["Public", "Register", "Private"] as const;

export type TimelineVisibility = (typeof kTimelineVisibilities)[number];

export interface HttpTimelineInfo {
  uniqueId: string;
  title: string;
  nameV2: string;
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
  deleted: false;
  time: string;
  author: HttpUser;
  dataList: HttpTimelinePostDataDigest[];
  color: string;
  lastUpdated: string;
  timelineOwnerV2: string;
  timelineNameV2: string;
  editable: boolean;
}

export interface HttpTimelineDeletedPostInfo {
  id: number;
  deleted: true;
  time: string;
  author?: HttpUser;
  dataList: HttpTimelinePostDataDigest[];
  color?: string;
  lastUpdated: string;
  timelineOwnerV2: string;
  timelineNameV2: string;
  editable: boolean;
}

export type HttpTimelineGenericPostInfo =
  | HttpTimelinePostInfo
  | HttpTimelineDeletedPostInfo;

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
  color?: string;
  visibility?: TimelineVisibility;
  description?: string;
}

export interface HttpTimelinePostPatchRequest {
  time?: string;
  color?: string;
}

export interface IHttpTimelineClient {
  listTimeline(query: HttpTimelineListQuery): Promise<HttpTimelineInfo[]>;
  getTimeline(
    ownerUsername: string,
    timelineName: string
  ): Promise<HttpTimelineInfo>;
  postTimeline(req: HttpTimelinePostRequest): Promise<HttpTimelineInfo>;
  patchTimeline(
    ownerUsername: string,
    timelineName: string,
    req: HttpTimelinePatchRequest
  ): Promise<HttpTimelineInfo>;
  deleteTimeline(ownerUsername: string, timelineName: string): Promise<void>;
  memberPut(
    ownerUsername: string,
    timelineName: string,
    username: string
  ): Promise<void>;
  memberDelete(
    ownerUsername: string,
    timelineName: string,
    username: string
  ): Promise<void>;
  listPost(
    ownerUsername: string,
    timelineName: string,
    page?: number,
    pageSize?: number
  ): Promise<Page<HttpTimelineGenericPostInfo>>;
  generatePostDataUrl(
    ownerUsername: string,
    timelineName: string,
    postId: number
  ): string;
  getPostDataAsString(
    ownerUsername: string,
    timelineName: string,
    postId: number
  ): Promise<string>;
  postPost(
    ownerUsername: string,
    timelineName: string,
    req: HttpTimelinePostPostRequest
  ): Promise<HttpTimelinePostInfo>;
  patchPost(
    ownerUsername: string,
    timelineName: string,
    postId: number,
    req: HttpTimelinePostPatchRequest
  ): Promise<HttpTimelinePostInfo>;
  deletePost(
    ownerUsername: string,
    timelineName: string,
    postId: number
  ): Promise<void>;
}

export class HttpTimelineClient implements IHttpTimelineClient {
  listTimeline(query: HttpTimelineListQuery): Promise<HttpTimelineInfo[]> {
    return axios
      .get<HttpTimelineInfo[]>(withQuery(`${apiBaseUrl}/timelines`, query))
      .then(extractResponseData);
  }

  getTimeline(
    ownerUsername: string,
    timelineName: string
  ): Promise<HttpTimelineInfo> {
    return axios
      .get<HttpTimelineInfo>(
        `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}`
      )
      .then(extractResponseData);
  }

  postTimeline(req: HttpTimelinePostRequest): Promise<HttpTimelineInfo> {
    return axios
      .post<HttpTimelineInfo>(`${apiBaseUrl}/v2/timelines`, req)
      .then(extractResponseData);
  }

  patchTimeline(
    ownerUsername: string,
    timelineName: string,
    req: HttpTimelinePatchRequest
  ): Promise<HttpTimelineInfo> {
    return axios
      .patch<HttpTimelineInfo>(
        `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}`,
        req
      )
      .then(extractResponseData);
  }

  deleteTimeline(ownerUsername: string, timelineName: string): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}`)
      .then();
  }

  memberPut(
    ownerUsername: string,
    timelineName: string,
    username: string
  ): Promise<void> {
    return axios
      .put(
        `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}/members/${username}`
      )
      .then();
  }

  memberDelete(
    ownerUsername: string,
    timelineName: string,
    username: string
  ): Promise<void> {
    return axios
      .delete(
        `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}/members/${username}`
      )
      .then();
  }

  listPost(
    ownerUsername: string,
    timelineName: string,
    page?: number,
    pageSize?: number
  ): Promise<Page<HttpTimelineGenericPostInfo>> {
    return axios
      .get<Page<HttpTimelineGenericPostInfo>>(
        withQuery(
          `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}/posts`,
          {
            page,
            pageSize,
          }
        )
      )
      .then(extractResponseData);
  }

  generatePostDataUrl(
    ownerUsername: string,
    timelineName: string,
    postId: number
  ): string {
    return withQuery(
      `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}/posts/${postId}/data`,
      { token: getHttpToken() }
    );
  }

  getPostDataAsString(
    ownerUsername: string,
    timelineName: string,
    postId: number
  ): Promise<string> {
    return axios
      .get<string>(
        `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}/posts/${postId}/data`,
        {
          responseType: "text",
        }
      )
      .then(extractResponseData);
  }

  postPost(
    ownerUsername: string,
    timelineName: string,
    req: HttpTimelinePostPostRequest
  ): Promise<HttpTimelinePostInfo> {
    return axios
      .post<HttpTimelinePostInfo>(
        `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}/posts`,
        req
      )
      .then(extractResponseData);
  }

  patchPost(
    ownerUsername: string,
    timelineName: string,
    postId: number,
    req: HttpTimelinePostPatchRequest
  ): Promise<HttpTimelinePostInfo> {
    return axios
      .patch<HttpTimelinePostInfo>(
        `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}/posts/${postId}`,
        req
      )
      .then(extractResponseData);
  }

  deletePost(
    ownerUsername: string,
    timelineName: string,
    postId: number
  ): Promise<void> {
    return axios
      .delete(
        `${apiBaseUrl}/v2/timelines/${ownerUsername}/${timelineName}/posts/${postId}`
      )
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
