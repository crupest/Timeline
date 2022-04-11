import { applyQueryParameters } from "@/utilities/url";
import { axios, apiBaseUrl, extractResponseData, Page } from "./common";

export interface TimelineBookmark {
  timelineOwner: string;
  timelineName: string;
  position: number;
}

export interface IHttpBookmarkClient {
  list(
    username: string,
    page?: number,
    pageSize?: number
  ): Promise<Page<TimelineBookmark>>;
  post(
    username: string,
    timelineOwner: string,
    timelineName: string
  ): Promise<TimelineBookmark>;
  delete(
    username: string,
    timelineOwner: string,
    timelineName: string
  ): Promise<void>;
  move(
    username: string,
    timelineOwner: string,
    timelineName: string,
    position: number
  ): Promise<TimelineBookmark>;
}

export class HttpHighlightClient implements IHttpBookmarkClient {
  list(
    username: string,
    page?: number,
    pageSize?: number
  ): Promise<Page<TimelineBookmark>> {
    const url = applyQueryParameters(
      `${apiBaseUrl}/v2/users/${username}/bookmarks`,
      { page, pageSize }
    );

    return axios.get<Page<TimelineBookmark>>(url).then(extractResponseData);
  }

  post(
    username: string,
    timelineOwner: string,
    timelineName: string
  ): Promise<TimelineBookmark> {
    const url = `${apiBaseUrl}/v2/users/${username}/bookmarks`;

    return axios
      .post<TimelineBookmark>(url, {
        timelineOwner,
        timelineName,
      })
      .then(extractResponseData);
  }

  delete(
    username: string,
    timelineOwner: string,
    timelineName: string
  ): Promise<void> {
    const url = `${apiBaseUrl}/v2/users/${username}/bookmarks/delete`;

    return axios.post(url, {
      timelineOwner,
      timelineName,
    });
  }

  move(
    username: string,
    timelineOwner: string,
    timelineName: string,
    position: number
  ): Promise<TimelineBookmark> {
    const url = `${apiBaseUrl}/v2/users/${username}/bookmarks/move`;

    return axios
      .post<TimelineBookmark>(url, {
        timelineOwner,
        timelineName,
        position,
      })
      .then(extractResponseData);
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
