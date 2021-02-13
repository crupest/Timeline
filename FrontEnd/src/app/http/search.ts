import { apiBaseUrl, axios, extractResponseData } from "./common";
import { HttpTimelineInfo } from "./timeline";
import { HttpUser } from "./user";

export interface IHttpSearchClient {
  searchTimelines(query: string): Promise<HttpTimelineInfo[]>;
  searchUsers(query: string): Promise<HttpUser[]>;
}

export class HttpSearchClient implements IHttpSearchClient {
  searchTimelines(query: string): Promise<HttpTimelineInfo[]> {
    return axios
      .get<HttpTimelineInfo[]>(`${apiBaseUrl}/search/timelines?q=${query}`)
      .then(extractResponseData);
  }

  searchUsers(query: string): Promise<HttpUser[]> {
    return axios
      .get<HttpUser[]>(`${apiBaseUrl}/search/users?q=${query}`)
      .then(extractResponseData);
  }
}

let client: IHttpSearchClient = new HttpSearchClient();

export function getHttpSearchClient(): IHttpSearchClient {
  return client;
}

export function setHttpSearchClient(
  newClient: IHttpSearchClient
): IHttpSearchClient {
  const old = client;
  client = newClient;
  return old;
}
