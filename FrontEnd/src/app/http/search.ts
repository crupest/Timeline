import {
  apiBaseUrl,
  axios,
  convertToNetworkError,
  extractResponseData,
} from "./common";
import {
  HttpTimelineInfo,
  processRawTimelineInfo,
  RawHttpTimelineInfo,
} from "./timeline";
import { HttpUser } from "./user";

export interface IHttpSearchClient {
  searchTimelines(query: string): Promise<HttpTimelineInfo[]>;
  searchUsers(query: string): Promise<HttpUser[]>;
}

export class HttpSearchClient implements IHttpSearchClient {
  searchTimelines(query: string): Promise<HttpTimelineInfo[]> {
    return axios
      .get<RawHttpTimelineInfo[]>(`${apiBaseUrl}/search/timelines?q=${query}`)
      .then(extractResponseData)
      .then((ts) => ts.map(processRawTimelineInfo))
      .catch(convertToNetworkError);
  }

  searchUsers(query: string): Promise<HttpUser[]> {
    return axios
      .get<HttpUser[]>(`${apiBaseUrl}/search/users?q=${query}`)
      .then(extractResponseData)
      .catch(convertToNetworkError);
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
