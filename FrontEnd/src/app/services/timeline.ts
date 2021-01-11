import React from "react";
import XRegExp from "xregexp";
import { Observable, from } from "rxjs";

import { convertError } from "@/utilities/rxjs";
import {
  TimelineVisibility,
  HttpTimelineInfo,
  HttpTimelinePatchRequest,
  HttpTimelinePostPostRequest,
  HttpTimelinePostPostRequestContent,
  HttpTimelinePostPostRequestTextContent,
  HttpTimelinePostPostRequestImageContent,
  HttpTimelinePostInfo,
  HttpTimelinePostTextContent,
  getHttpTimelineClient,
  HttpTimelineNotExistError,
  HttpTimelineNameConflictError,
} from "@/http/timeline";
import { HttpForbiddenError, HttpNetworkError } from "@/http/common";

export { kTimelineVisibilities } from "@/http/timeline";

export type { TimelineVisibility } from "@/http/timeline";

import { dataStorage } from "./common";
import { userInfoService, AuthUser } from "./user";
import { DataAndStatus, DataHub2 } from "./DataHub2";

export type TimelineInfo = HttpTimelineInfo;
export type TimelineChangePropertyRequest = HttpTimelinePatchRequest;
export type TimelineCreatePostRequest = HttpTimelinePostPostRequest;
export type TimelineCreatePostContent = HttpTimelinePostPostRequestContent;
export type TimelineCreatePostTextContent = HttpTimelinePostPostRequestTextContent;
export type TimelineCreatePostImageContent = HttpTimelinePostPostRequestImageContent;

export type TimelinePostTextContent = HttpTimelinePostTextContent;

export interface TimelinePostImageContent {
  type: "image";
  data: Blob;
  etag: string;
}

export type TimelinePostContent =
  | TimelinePostTextContent
  | TimelinePostImageContent;

export type TimelinePostInfo = Omit<HttpTimelinePostInfo, "content"> & {
  content: TimelinePostContent;
};

export interface TimelinePostsInfo {
  lastUpdated: Date;
  posts: TimelinePostInfo[];
}

export const timelineVisibilityTooltipTranslationMap: Record<
  TimelineVisibility,
  string
> = {
  Public: "timeline.visibilityTooltip.public",
  Register: "timeline.visibilityTooltip.register",
  Private: "timeline.visibilityTooltip.private",
};

export class TimelineNameConflictError extends Error {}

type TimelineData = Omit<HttpTimelineInfo, "owner" | "members"> & {
  owner: string;
  members: string[];
};

type TimelinePostData = Omit<TimelinePostInfo, "author"> & {
  author: string;
};

interface TimelinePostsData {
  lastUpdated: Date;
  posts: TimelinePostData[];
}

export class TimelineService {
  private async clearTimelineData(timelineName: string): Promise<void> {
    const keys = (await dataStorage.keys()).filter((k) =>
      k.startsWith(`timeline.${timelineName}`)
    );
    await Promise.all(keys.map((k) => dataStorage.removeItem(k)));
  }

  private generateTimelineDataStorageKey(timelineName: string): string {
    return `timeline.${timelineName}`;
  }

  private convertHttpTimelineToData(timeline: HttpTimelineInfo): TimelineData {
    return {
      ...timeline,
      owner: timeline.owner.username,
      members: timeline.members.map((m) => m.username),
    };
  }

  readonly timelineHub = new DataHub2<string, HttpTimelineInfo | "notexist">({
    saveData: async (timelineName, data) => {
      if (data === "notexist") return;

      userInfoService.saveUser(data.owner);
      userInfoService.saveUsers(data.members);

      await dataStorage.setItem<TimelineData>(
        this.generateTimelineDataStorageKey(timelineName),
        this.convertHttpTimelineToData(data)
      );
    },
    getSavedData: async (timelineName) => {
      const savedData = await dataStorage.getItem<TimelineData | null>(
        this.generateTimelineDataStorageKey(timelineName)
      );

      if (savedData == null) return null;

      const owner = await userInfoService.getCachedUser(savedData.owner);
      if (owner == null) return null;
      const members = await userInfoService.getCachedUsers(savedData.members);
      if (members == null) return null;

      return { ...savedData, owner, members };
    },
    fetchData: async (timelineName, savedData) => {
      try {
        const timeline = await getHttpTimelineClient().getTimeline(
          timelineName
        );

        if (
          savedData != null &&
          savedData !== "notexist" &&
          savedData.uniqueId !== timeline.uniqueId
        ) {
          console.log(
            `Timeline with name ${timelineName} has changed to a new one. Clear old data.`
          );

          void this.clearTimelineData(timelineName); // If timeline has changed, clear all old data.
        }

        return timeline;
      } catch (e) {
        if (e instanceof HttpTimelineNotExistError) {
          return "notexist";
        } else if (e instanceof HttpNetworkError) {
          return null;
        } else {
          throw e;
        }
      }
    },
  });

  syncTimeline(timelineName: string): void {
    this.timelineHub.getLine(timelineName).sync();
  }

  createTimeline(timelineName: string): Observable<TimelineInfo> {
    return from(
      getHttpTimelineClient().postTimeline({
        name: timelineName,
      })
    ).pipe(
      convertError(HttpTimelineNameConflictError, TimelineNameConflictError)
    );
  }

  changeTimelineProperty(
    timelineName: string,
    req: TimelineChangePropertyRequest
  ): Observable<TimelineInfo> {
    return from(
      getHttpTimelineClient()
        .patchTimeline(timelineName, req)
        .then((timeline) => {
          void this.syncTimeline(timelineName);
          return timeline;
        })
    );
  }

  deleteTimeline(timelineName: string): Observable<unknown> {
    return from(getHttpTimelineClient().deleteTimeline(timelineName));
  }

  addMember(timelineName: string, username: string): Promise<void> {
    return getHttpTimelineClient()
      .memberPut(timelineName, username)
      .then(() => {
        void this.syncTimeline(timelineName);
      });
  }

  removeMember(timelineName: string, username: string): Promise<void> {
    return getHttpTimelineClient()
      .memberDelete(timelineName, username)
      .then(() => {
        void this.syncTimeline(timelineName);
      });
  }

  private generatePostsDataStorageKey(timelineName: string): string {
    return `timeline.${timelineName}.posts`;
  }

  readonly postsHub = new DataHub2<
    string,
    TimelinePostsInfo | "notexist" | "forbid"
  >({
    saveData: async (timelineName, data) => {
      if (data === "notexist" || data === "forbid") return;

      const savedData: TimelinePostsData = {
        ...data,
        posts: data.posts.map((p) => ({ ...p, author: p.author.username })),
      };

      data.posts.forEach((p) => {
        userInfoService.saveUser(p.author);
      });

      await dataStorage.setItem<TimelinePostsData>(
        this.generatePostsDataStorageKey(timelineName),
        savedData
      );
    },
    getSavedData: async (timelineName) => {
      const savedData = await dataStorage.getItem<TimelinePostsData | null>(
        this.generatePostsDataStorageKey(timelineName)
      );
      if (savedData == null) return null;

      const authors = await userInfoService.getCachedUsers(
        savedData.posts.map((p) => p.author)
      );

      if (authors == null) return null;

      return {
        ...savedData,
        posts: savedData.posts.map((p, index) => ({
          ...p,
          author: authors[index],
        })),
      };
    },
    fetchData: async (timelineName, savedData) => {
      const convert = async (
        post: HttpTimelinePostInfo
      ): Promise<TimelinePostInfo> => {
        const { content } = post;
        if (content.type === "text") {
          return { ...post, content };
        } else {
          const data = await getHttpTimelineClient().getPostData(
            timelineName,
            post.id
          );
          return {
            ...post,
            content: {
              type: "image",
              data: data.data,
              etag: data.etag,
            },
          };
        }
      };

      const convertList = (
        posts: HttpTimelinePostInfo[]
      ): Promise<TimelinePostInfo[]> =>
        Promise.all(posts.map((p) => convert(p)));

      const now = new Date();

      try {
        if (
          savedData == null ||
          savedData === "forbid" ||
          savedData === "notexist"
        ) {
          const httpPosts = await getHttpTimelineClient().listPost(
            timelineName
          );

          return {
            lastUpdated: now,
            posts: await convertList(httpPosts),
          };
        } else {
          const httpPosts = await getHttpTimelineClient().listPost(
            timelineName,
            {
              modifiedSince: savedData.lastUpdated,
              includeDeleted: true,
            }
          );

          const deletedIds = httpPosts
            .filter((p) => p.deleted)
            .map((p) => p.id);

          const changed = await convertList(
            httpPosts.filter((p): p is HttpTimelinePostInfo => !p.deleted)
          );

          const posts = savedData.posts.filter(
            (p) => !deletedIds.includes(p.id)
          );

          for (const changedPost of changed) {
            const savedChangedPostIndex = posts.findIndex(
              (p) => p.id === changedPost.id
            );
            if (savedChangedPostIndex === -1) {
              posts.push(await convert(changedPost));
            } else {
              posts[savedChangedPostIndex] = await convert(changedPost);
            }
          }

          return { lastUpdated: now, posts };
        }
      } catch (e) {
        if (e instanceof HttpTimelineNotExistError) {
          return "notexist";
        } else if (e instanceof HttpForbiddenError) {
          return "forbid";
        } else if (e instanceof HttpNetworkError) {
          return null;
        } else {
          throw e;
        }
      }
    },
  });

  syncPosts(timelineName: string): void {
    this.postsHub.getLine(timelineName).sync();
  }

  createPost(
    timelineName: string,
    request: TimelineCreatePostRequest
  ): Observable<unknown> {
    return from(
      getHttpTimelineClient()
        .postPost(timelineName, request)
        .then(() => {
          this.syncPosts(timelineName);
        })
    );
  }

  deletePost(timelineName: string, postId: number): Observable<unknown> {
    return from(
      getHttpTimelineClient()
        .deletePost(timelineName, postId)
        .then(() => {
          this.syncPosts(timelineName);
        })
    );
  }

  isMemberOf(username: string, timeline: TimelineInfo): boolean {
    return timeline.members.findIndex((m) => m.username == username) >= 0;
  }

  hasReadPermission(
    user: AuthUser | null | undefined,
    timeline: TimelineInfo
  ): boolean {
    if (user != null && user.hasAllTimelineAdministrationPermission)
      return true;

    const { visibility } = timeline;
    if (visibility === "Public") {
      return true;
    } else if (visibility === "Register") {
      if (user != null) return true;
    } else if (visibility === "Private") {
      if (
        user != null &&
        (user.username === timeline.owner.username ||
          this.isMemberOf(user.username, timeline))
      ) {
        return true;
      }
    }
    return false;
  }

  hasPostPermission(
    user: AuthUser | null | undefined,
    timeline: TimelineInfo
  ): boolean {
    if (user != null && user.hasAllTimelineAdministrationPermission)
      return true;

    return (
      user != null &&
      (timeline.owner.username === user.username ||
        this.isMemberOf(user.username, timeline))
    );
  }

  hasManagePermission(
    user: AuthUser | null | undefined,
    timeline: TimelineInfo
  ): boolean {
    if (user != null && user.hasAllTimelineAdministrationPermission)
      return true;

    return user != null && user.username == timeline.owner.username;
  }

  hasModifyPostPermission(
    user: AuthUser | null | undefined,
    timeline: TimelineInfo,
    post: TimelinePostInfo
  ): boolean {
    if (user != null && user.hasAllTimelineAdministrationPermission)
      return true;

    return (
      user != null &&
      (user.username === timeline.owner.username ||
        user.username === post.author.username)
    );
  }
}

export const timelineService = new TimelineService();

const timelineNameReg = XRegExp("^[-_\\p{L}]*$", "u");

export function validateTimelineName(name: string): boolean {
  return timelineNameReg.test(name);
}

export function useTimeline(
  timelineName: string
): DataAndStatus<TimelineInfo | "notexist"> {
  const [state, setState] = React.useState<
    DataAndStatus<TimelineInfo | "notexist">
  >({
    status: "syncing",
    data: null,
  });
  React.useEffect(() => {
    const subscription = timelineService.timelineHub
      .getLine(timelineName)
      .getObservalble()
      .subscribe((data) => {
        setState(data);
      });
    return () => {
      subscription.unsubscribe();
    };
  }, [timelineName]);
  return state;
}

export function usePosts(
  timelineName: string
): DataAndStatus<TimelinePostsInfo | "notexist" | "forbid"> {
  const [state, setState] = React.useState<
    DataAndStatus<TimelinePostsInfo | "notexist" | "forbid">
  >({ status: "syncing", data: null });
  React.useEffect(() => {
    const subscription = timelineService.postsHub
      .getLine(timelineName)
      .getObservalble()
      .subscribe((data) => {
        setState(data);
      });
    return () => {
      subscription.unsubscribe();
    };
  }, [timelineName]);
  return state;
}

export async function getAllCachedTimelineNames(): Promise<string[]> {
  const keys = await dataStorage.keys();
  return keys
    .filter(
      (key) =>
        key.startsWith("timeline.") && (key.match(/\./g) ?? []).length === 1
    )
    .map((key) => key.substr("timeline.".length));
}
