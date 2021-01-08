import { AxiosError } from "axios";

import { updateQueryString, applyQueryParameters } from "../utilities/url";

import {
  axios,
  apiBaseUrl,
  extractResponseData,
  convertToNetworkError,
  base64,
  convertToIfStatusCodeIs,
  convertToIfErrorCodeIs,
  BlobWithEtag,
  NotModified,
  convertToNotModified,
  convertToForbiddenError,
  convertToBlobWithEtag,
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
  lastModified: Date;
  members: HttpUser[];
  isHighlight: boolean;
  isBookmark: boolean;
}

export interface HttpTimelineListQuery {
  visibility?: TimelineVisibility;
  relate?: string;
  relateType?: "own" | "join";
}

export interface HttpTimelinePostRequest {
  name: string;
}

export interface HttpTimelinePostTextContent {
  type: "text";
  text: string;
}

export interface HttpTimelinePostImageContent {
  type: "image";
}

export type HttpTimelinePostContent =
  | HttpTimelinePostTextContent
  | HttpTimelinePostImageContent;

export interface HttpTimelinePostInfo {
  id: number;
  content: HttpTimelinePostContent;
  time: Date;
  lastUpdated: Date;
  author: HttpUser;
  deleted: false;
}

export interface HttpTimelineDeletedPostInfo {
  id: number;
  time: Date;
  lastUpdated: Date;
  author?: HttpUser;
  deleted: true;
}

export type HttpTimelineGenericPostInfo =
  | HttpTimelinePostInfo
  | HttpTimelineDeletedPostInfo;

export interface HttpTimelinePostPostRequestTextContent {
  type: "text";
  text: string;
}

export interface HttpTimelinePostPostRequestImageContent {
  type: "image";
  data: Blob;
}

export type HttpTimelinePostPostRequestContent =
  | HttpTimelinePostPostRequestTextContent
  | HttpTimelinePostPostRequestImageContent;

export interface HttpTimelinePostPostRequest {
  content: HttpTimelinePostPostRequestContent;
  time?: Date;
}

export interface HttpTimelinePatchRequest {
  title?: string;
  visibility?: TimelineVisibility;
  description?: string;
}

export class HttpTimelineNotExistError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export class HttpTimelinePostNotExistError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

export class HttpTimelineNameConflictError extends Error {
  constructor(public innerError?: AxiosError) {
    super();
  }
}

//-------------------- begin: internal model --------------------

export interface RawHttpTimelineInfo {
  uniqueId: string;
  title: string;
  name: string;
  description: string;
  owner: HttpUser;
  visibility: TimelineVisibility;
  lastModified: string;
  members: HttpUser[];
  isHighlight: boolean;
  isBookmark: boolean;
}

interface RawTimelinePostTextContent {
  type: "text";
  text: string;
}

interface RawTimelinePostImageContent {
  type: "image";
  url: string;
}

type RawTimelinePostContent =
  | RawTimelinePostTextContent
  | RawTimelinePostImageContent;

interface RawTimelinePostInfo {
  id: number;
  content: RawTimelinePostContent;
  time: string;
  lastUpdated: string;
  author: HttpUser;
  deleted: false;
}

interface RawTimelineDeletedPostInfo {
  id: number;
  time: string;
  lastUpdated: string;
  author: HttpUser;
  deleted: true;
}

type RawTimelineGenericPostInfo =
  | RawTimelinePostInfo
  | RawTimelineDeletedPostInfo;

interface RawTimelinePostPostRequestTextContent {
  type: "text";
  text: string;
}

interface RawTimelinePostPostRequestImageContent {
  type: "image";
  data: string;
}

type RawTimelinePostPostRequestContent =
  | RawTimelinePostPostRequestTextContent
  | RawTimelinePostPostRequestImageContent;

interface RawTimelinePostPostRequest {
  content: RawTimelinePostPostRequestContent;
  time?: string;
}

//-------------------- end: internal model --------------------

export function processRawTimelineInfo(
  raw: RawHttpTimelineInfo
): HttpTimelineInfo {
  return {
    ...raw,
    lastModified: new Date(raw.lastModified),
  };
}

function processRawTimelinePostInfo(
  raw: RawTimelinePostInfo
): HttpTimelinePostInfo;
function processRawTimelinePostInfo(
  raw: RawTimelineGenericPostInfo
): HttpTimelineGenericPostInfo;
function processRawTimelinePostInfo(
  raw: RawTimelineGenericPostInfo
): HttpTimelineGenericPostInfo {
  return {
    ...raw,
    time: new Date(raw.time),
    lastUpdated: new Date(raw.lastUpdated),
  };
}

export interface IHttpTimelineClient {
  listTimeline(query: HttpTimelineListQuery): Promise<HttpTimelineInfo[]>;
  getTimeline(timelineName: string): Promise<HttpTimelineInfo>;
  getTimeline(
    timelineName: string,
    query: {
      checkUniqueId?: string;
    }
  ): Promise<HttpTimelineInfo>;
  getTimeline(
    timelineName: string,
    query: {
      checkUniqueId?: string;
      ifModifiedSince: Date;
    }
  ): Promise<HttpTimelineInfo | NotModified>;
  postTimeline(req: HttpTimelinePostRequest): Promise<HttpTimelineInfo>;
  patchTimeline(
    timelineName: string,
    req: HttpTimelinePatchRequest
  ): Promise<HttpTimelineInfo>;
  deleteTimeline(timelineName: string): Promise<void>;
  memberPut(timelineName: string, username: string): Promise<void>;
  memberDelete(timelineName: string, username: string): Promise<void>;
  listPost(timelineName: string): Promise<HttpTimelinePostInfo[]>;
  listPost(
    timelineName: string,
    query: {
      modifiedSince?: Date;
      includeDeleted?: false;
    }
  ): Promise<HttpTimelinePostInfo[]>;
  listPost(
    timelineName: string,
    query: {
      modifiedSince?: Date;
      includeDeleted: true;
    }
  ): Promise<HttpTimelineGenericPostInfo[]>;
  getPostData(timelineName: string, postId: number): Promise<BlobWithEtag>;
  getPostData(
    timelineName: string,
    postId: number,
    etag: string
  ): Promise<BlobWithEtag | NotModified>;
  postPost(
    timelineName: string,
    req: HttpTimelinePostPostRequest
  ): Promise<HttpTimelinePostInfo>;
  deletePost(timelineName: string, postId: number): Promise<void>;
}

export class HttpTimelineClient implements IHttpTimelineClient {
  listTimeline(query: HttpTimelineListQuery): Promise<HttpTimelineInfo[]> {
    return axios
      .get<RawHttpTimelineInfo[]>(
        applyQueryParameters(`${apiBaseUrl}/timelines`, query)
      )
      .then(extractResponseData)
      .then((list) => list.map(processRawTimelineInfo))
      .catch(convertToNetworkError);
  }

  getTimeline(timelineName: string): Promise<HttpTimelineInfo>;
  getTimeline(
    timelineName: string,
    query: {
      checkUniqueId?: string;
    }
  ): Promise<HttpTimelineInfo>;
  getTimeline(
    timelineName: string,
    query: {
      checkUniqueId?: string;
      ifModifiedSince: Date;
    }
  ): Promise<HttpTimelineInfo | NotModified>;
  getTimeline(
    timelineName: string,
    query?: {
      checkUniqueId?: string;
      ifModifiedSince?: Date;
    }
  ): Promise<HttpTimelineInfo | NotModified> {
    return axios
      .get<RawHttpTimelineInfo>(
        applyQueryParameters(`${apiBaseUrl}/timelines/${timelineName}`, query)
      )
      .then((res) => {
        if (res.status === 304) {
          return new NotModified();
        } else {
          return processRawTimelineInfo(res.data);
        }
      })
      .catch(convertToIfStatusCodeIs(404, HttpTimelineNotExistError))
      .catch(convertToNetworkError);
  }

  postTimeline(req: HttpTimelinePostRequest): Promise<HttpTimelineInfo> {
    return axios
      .post<RawHttpTimelineInfo>(`${apiBaseUrl}/timelines`, req)
      .then(extractResponseData)
      .then(processRawTimelineInfo)
      .catch(convertToIfErrorCodeIs(11040101, HttpTimelineNameConflictError))
      .catch(convertToNetworkError);
  }

  patchTimeline(
    timelineName: string,
    req: HttpTimelinePatchRequest
  ): Promise<HttpTimelineInfo> {
    return axios
      .patch<RawHttpTimelineInfo>(
        `${apiBaseUrl}/timelines/${timelineName}`,
        req
      )
      .then(extractResponseData)
      .then(processRawTimelineInfo)
      .catch(convertToNetworkError);
  }

  deleteTimeline(timelineName: string): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/timelines/${timelineName}`)
      .catch(convertToNetworkError)
      .then();
  }

  memberPut(timelineName: string, username: string): Promise<void> {
    return axios
      .put(`${apiBaseUrl}/timelines/${timelineName}/members/${username}`)
      .catch(convertToNetworkError)
      .then();
  }

  memberDelete(timelineName: string, username: string): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/timelines/${timelineName}/members/${username}`)
      .catch(convertToNetworkError)
      .then();
  }

  listPost(timelineName: string): Promise<HttpTimelinePostInfo[]>;
  listPost(
    timelineName: string,
    query: {
      modifiedSince?: Date;
      includeDeleted?: false;
    }
  ): Promise<HttpTimelinePostInfo[]>;
  listPost(
    timelineName: string,
    query: {
      modifiedSince?: Date;
      includeDeleted: true;
    }
  ): Promise<HttpTimelineGenericPostInfo[]>;
  listPost(
    timelineName: string,
    query?: {
      modifiedSince?: Date;
      includeDeleted?: boolean;
    }
  ): Promise<HttpTimelineGenericPostInfo[]> {
    let url = `${apiBaseUrl}/timelines/${timelineName}/posts`;
    if (query != null) {
      if (query.modifiedSince != null) {
        url = updateQueryString(
          "modifiedSince",
          query.modifiedSince.toISOString(),
          url
        );
      }
      if (query.includeDeleted != null) {
        url = updateQueryString(
          "includeDeleted",
          query.includeDeleted ? "true" : "false",
          url
        );
      }
    }

    return axios
      .get<RawTimelineGenericPostInfo[]>(url)
      .then(extractResponseData)
      .catch(convertToIfStatusCodeIs(404, HttpTimelineNotExistError))
      .catch(convertToForbiddenError)
      .catch(convertToNetworkError)
      .then((rawPosts) =>
        rawPosts.map((raw) => processRawTimelinePostInfo(raw))
      );
  }

  getPostData(timelineName: string, postId: number): Promise<BlobWithEtag>;
  getPostData(
    timelineName: string,
    postId: number,
    etag?: string
  ): Promise<BlobWithEtag | NotModified> {
    const headers =
      etag != null
        ? {
            "If-None-Match": etag,
          }
        : undefined;

    const url = `${apiBaseUrl}/timelines/${timelineName}/posts/${postId}/data`;

    return axios
      .get(url, {
        responseType: "blob",
        headers,
      })
      .then(convertToBlobWithEtag)
      .catch(convertToNotModified)
      .catch(convertToIfStatusCodeIs(404, HttpTimelinePostNotExistError))
      .catch(convertToNetworkError);
  }

  async postPost(
    timelineName: string,
    req: HttpTimelinePostPostRequest
  ): Promise<HttpTimelinePostInfo> {
    let content: RawTimelinePostPostRequestContent;
    if (req.content.type === "image") {
      const base64Data = await base64(req.content.data);
      content = {
        ...req.content,
        data: base64Data,
      } as RawTimelinePostPostRequestImageContent;
    } else {
      content = req.content;
    }
    const rawReq: RawTimelinePostPostRequest = {
      content,
    };
    if (req.time != null) {
      rawReq.time = req.time.toISOString();
    }
    return await axios
      .post<RawTimelinePostInfo>(
        `${apiBaseUrl}/timelines/${timelineName}/posts`,
        rawReq
      )
      .then(extractResponseData)
      .catch(convertToNetworkError)
      .then((rawPost) => processRawTimelinePostInfo(rawPost));
  }

  deletePost(timelineName: string, postId: number): Promise<void> {
    return axios
      .delete(`${apiBaseUrl}/timelines/${timelineName}/posts/${postId}`)
      .catch(convertToNetworkError)
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
