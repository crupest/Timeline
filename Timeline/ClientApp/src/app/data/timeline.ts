import React from 'react';
import XRegExp from 'xregexp';
import { Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';

import { convertError } from '../utilities/rxjs';

import { dataStorage } from './common';
import { SubscriptionHub, ISubscriptionHub } from './SubscriptionHub';

import { UserAuthInfo, checkLogin, userService, userInfoService } from './user';

export { kTimelineVisibilities } from '../http/timeline';

export type { TimelineVisibility } from '../http/timeline';

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
} from '../http/timeline';
import {
  BlobWithEtag,
  NotModified,
  HttpNetworkError,
  HttpForbiddenError,
} from '../http/common';
import { HttpUser } from '../http/user';

export type TimelineInfo = HttpTimelineInfo;
export type TimelineChangePropertyRequest = HttpTimelinePatchRequest;
export type TimelineCreatePostRequest = HttpTimelinePostPostRequest;
export type TimelineCreatePostContent = HttpTimelinePostPostRequestContent;
export type TimelineCreatePostTextContent = HttpTimelinePostPostRequestTextContent;
export type TimelineCreatePostImageContent = HttpTimelinePostPostRequestImageContent;

export type TimelinePostTextContent = HttpTimelinePostTextContent;

export interface TimelinePostImageContent {
  type: 'image';
  data: Blob;
}

export type TimelinePostContent =
  | TimelinePostTextContent
  | TimelinePostImageContent;

export interface TimelinePostInfo {
  id: number;
  content: TimelinePostContent;
  time: Date;
  lastUpdated: Date;
  author: HttpUser;
}

export const timelineVisibilityTooltipTranslationMap: Record<
  TimelineVisibility,
  string
> = {
  Public: 'timeline.visibilityTooltip.public',
  Register: 'timeline.visibilityTooltip.register',
  Private: 'timeline.visibilityTooltip.private',
};

export class TimelineNotExistError extends Error {}
export class TimelineNameConflictError extends Error {}

export type TimelineWithSyncState =
  | {
      syncState: 'loadcache'; // Loading cache now.
      timeline?: undefined;
    }
  | {
      syncState:
        | 'syncing' // Cache loaded and syncing for the first time.
        | 'offline' // Sync failed and use cache. Null timeline means no cache.
        | 'synced' // Sync succeeded. Null timeline means the timeline does not exist.
        | 'new'; // This is a new timeline different from cached one. Null timeline means the timeline does not exist.
      timeline: TimelineInfo | null;
    };

export interface TimelinePostsWithSyncState {
  state:
    | 'loadcache'
    | 'syncing' // Syncing now.
    | 'offline' // Sync failed and use cache.
    | 'synced' // Sync succeeded.
    | 'forbid' // The list is forbidden to see.
    | 'notexist'; // The timeline does not exist.
  posts: TimelinePostInfo[];
}

type FetchAndCacheTimelineResult = TimelineInfo | 'offline' | 'notexist';

type FetchAndCachePostsResult =
  | TimelinePostInfo[]
  | 'notexist'
  | 'forbid'
  | 'offline';

export class TimelineService {
  private getTimelineKey(timelineName: string): string {
    return `timeline.${timelineName}`;
  }

  private getCachedTimeline(
    timelineName: string
  ): Promise<TimelineInfo | null> {
    return dataStorage.getItem<TimelineInfo | null>(
      this.getTimelineKey(timelineName)
    );
  }

  private async fetchAndCacheTimeline(
    timelineName: string
  ): Promise<FetchAndCacheTimelineResult> {
    try {
      const timeline = await getHttpTimelineClient().getTimeline(timelineName);
      await dataStorage.setItem<TimelineInfo>(
        this.getTimelineKey(timelineName),
        timeline
      );
      return timeline;
    } catch (e) {
      if (e instanceof HttpTimelineNotExistError) {
        return 'notexist';
      } else if (e instanceof HttpNetworkError) {
        return 'offline';
      } else {
        throw e;
      }
    }
  }

  private async syncTimeline(timelineName: string): Promise<void> {
    const line = this._timelineSubscriptionHub.getLine(timelineName);

    if (line == null) {
      console.log('No subscription, skip sync!');
      return;
    }

    const old = line.value;

    if (
      old != null &&
      (old.syncState === 'loadcache' || old.syncState === 'syncing')
    ) {
      return;
    }

    const next = line.next.bind(line);

    if (old == undefined) {
      next({ syncState: 'loadcache' });
      const timeline = await this.getCachedTimeline(timelineName);
      next({ syncState: 'syncing', timeline });
    } else {
      next({ syncState: 'syncing', timeline: old?.timeline });
    }

    const result = await this.fetchAndCacheTimeline(timelineName);

    if (result === 'offline') {
      next({ syncState: 'offline', timeline: line.value?.timeline ?? null });
    } else if (result === 'notexist') {
      if (line.value?.timeline != null) {
        next({ syncState: 'new', timeline: null });
      } else {
        next({ syncState: 'synced', timeline: null });
      }
    } else {
      if (result.uniqueId === line.value?.timeline?.uniqueId) {
        next({ syncState: 'synced', timeline: result });
      } else {
        next({ syncState: 'new', timeline: result });
      }
    }
  }

  private _timelineSubscriptionHub = new SubscriptionHub<
    string,
    TimelineWithSyncState
  >({
    setup: (key) => {
      void this.syncTimeline(key);
    },
    destroyable: (_, value) =>
      value?.syncState !== 'loadcache' && value?.syncState !== 'syncing',
  });

  get timelineHub(): ISubscriptionHub<string, TimelineWithSyncState> {
    return this._timelineSubscriptionHub;
  }

  createTimeline(timelineName: string): Observable<TimelineInfo> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient().postTimeline(
        {
          name: timelineName,
        },
        user.token
      )
    ).pipe(
      convertError(HttpTimelineNameConflictError, TimelineNameConflictError)
    );
  }

  changeTimelineProperty(
    timelineName: string,
    req: TimelineChangePropertyRequest
  ): Observable<TimelineInfo> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient()
        .patchTimeline(timelineName, req, user.token)
        .then((timeline) => {
          void this.syncTimeline(timelineName);
          return timeline;
        })
    );
  }

  deleteTimeline(timelineName: string): Observable<unknown> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient().deleteTimeline(timelineName, user.token)
    );
  }

  addMember(timelineName: string, username: string): Observable<unknown> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient()
        .memberPut(timelineName, username, user.token)
        .then(() => {
          userInfoService.getUserInfo(username).subscribe(() => {
            void this.syncTimeline(timelineName);
          });
        })
    );
  }

  removeMember(timelineName: string, username: string): Observable<unknown> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient()
        .memberDelete(timelineName, username, user.token)
        .then(() => {
          void this.syncTimeline(timelineName);
        })
    );
  }

  private getPostsKey(timelineName: string): string {
    return `timeline.${timelineName}.posts`;
  }

  private getPostDataKey(timelineName: string, id: number): string {
    return `timeline.${timelineName}.post.${id}.data`;
  }

  private convertPost = async (
    post: HttpTimelinePostInfo,
    dataProvider: () => Promise<Blob | null | undefined>
  ): Promise<TimelinePostInfo> => {
    const { content } = post;
    if (content.type === 'text') {
      return {
        ...post,
        content,
      };
    } else {
      const data = await dataProvider();
      if (data == null) throw new Error('This post requires data.');
      return {
        ...post,
        content: {
          type: 'image',
          data,
        },
      };
    }
  };

  private convertPostList = (
    posts: HttpTimelinePostInfo[],
    dataProvider: (
      post: HttpTimelinePostInfo,
      index: number
    ) => Promise<Blob | null | undefined>
  ): Promise<TimelinePostInfo[]> => {
    return Promise.all(
      posts.map((post, index) =>
        this.convertPost(post, () => dataProvider(post, index))
      )
    );
  };

  private async getCachedPosts(
    timelineName: string
  ): Promise<TimelinePostInfo[]> {
    const key = this.getPostsKey(timelineName);
    const httpPosts = await dataStorage.getItem<HttpTimelinePostInfo[] | null>(
      key
    );

    if (httpPosts == null) return [];

    const posts = await this.convertPostList(httpPosts, (post) =>
      dataStorage
        .getItem<BlobWithEtag | null>(
          this.getPostDataKey(timelineName, post.id)
        )
        .then((d) => d?.data)
    );

    return posts;
  }

  private async fetchAndCachePosts(
    timelineName: string,
    notUseDataCache = false
  ): Promise<FetchAndCachePostsResult> {
    try {
      const token = userService.currentUser?.token;

      const httpPosts = await getHttpTimelineClient().listPost(
        timelineName,
        token
      );

      const dataList: (
        | (BlobWithEtag & { cache: boolean })
        | null
      )[] = await Promise.all(
        httpPosts.map(async (post) => {
          const { content } = post;
          if (content.type === 'image') {
            if (notUseDataCache) {
              const data = await getHttpTimelineClient().getPostData(
                timelineName,
                post.id,
                token
              );
              return { ...data, cache: false };
            } else {
              const savedData = await dataStorage.getItem<BlobWithEtag | null>(
                this.getPostDataKey(timelineName, post.id)
              );
              if (savedData == null) {
                const data = await getHttpTimelineClient().getPostData(
                  timelineName,
                  post.id,
                  token
                );
                return { ...data, cache: false };
              } else {
                const res = await getHttpTimelineClient().getPostData(
                  timelineName,
                  post.id,
                  token,
                  savedData.etag
                );
                if (res instanceof NotModified) {
                  return { ...savedData, cache: true };
                } else {
                  await dataStorage.setItem<BlobWithEtag>(
                    this.getPostDataKey(timelineName, post.id),
                    res
                  );
                  return { ...res, cache: false };
                }
              }
            }
          } else {
            return null;
          }
        })
      );

      for (const [i, post] of httpPosts.entries()) {
        const data = dataList[i];
        if (data != null && !data.cache) {
          await dataStorage.setItem<BlobWithEtag>(
            this.getPostDataKey(timelineName, post.id),
            data
          );
        }
      }

      await dataStorage.setItem<HttpTimelinePostInfo[]>(
        this.getPostsKey(timelineName),
        httpPosts
      );

      const posts: TimelinePostInfo[] = await this.convertPostList(
        httpPosts,
        (post, i) => Promise.resolve(dataList[i]?.data)
      );

      return posts;
    } catch (e) {
      if (e instanceof HttpNetworkError) {
        return 'offline';
      } else if (e instanceof HttpForbiddenError) {
        return 'forbid';
      } else if (e instanceof HttpTimelineNotExistError) {
        return 'notexist';
      } else {
        throw e;
      }
    }
  }

  private async syncPosts(
    timelineName: string,
    notUseCachedData = false
  ): Promise<void> {
    const line = this._postsSubscriptionHub.getLine(timelineName);
    if (line == null) return;

    if (
      line.value != null &&
      (line.value.state === 'loadcache' || line.value.state === 'syncing')
    ) {
      return;
    }

    const next = (value: TimelinePostsWithSyncState): void => {
      line.next(value);
    };

    if (line.value == null) {
      next({
        state: 'loadcache',
        posts: [],
      });
      const posts = await this.getCachedPosts(timelineName);
      next({
        state: 'syncing',
        posts,
      });
    } else {
      next({
        state: 'syncing',
        posts: line.value.posts,
      });
    }

    const result = await this.fetchAndCachePosts(
      timelineName,
      notUseCachedData
    );
    if (result === 'offline') {
      next({ state: 'offline', posts: line.value?.posts ?? [] });
    } else if (Array.isArray(result)) {
      next({ state: 'synced', posts: result });
    } else {
      next({ state: result, posts: [] });
    }
  }

  private _postsSubscriptionHub = new SubscriptionHub<
    string,
    TimelinePostsWithSyncState
  >({
    setup: (key) => {
      void this.syncPosts(key);
    },
  });

  get postsHub(): ISubscriptionHub<string, TimelinePostsWithSyncState> {
    return this._postsSubscriptionHub;
  }

  createPost(
    timelineName: string,
    request: TimelineCreatePostRequest
  ): Observable<TimelinePostInfo> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient()
        .postPost(timelineName, request, user.token)
        .then((post) =>
          this.convertPost(post, () =>
            Promise.resolve(
              (request.content as TimelineCreatePostImageContent).data
            )
          )
        )
        .then((post) => {
          void this.syncPosts(timelineName);
          return post;
        })
    ).pipe(map((post) => ({ ...post, timelineName })));
  }

  deletePost(timelineName: string, postId: number): Observable<unknown> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient()
        .deletePost(timelineName, postId, user.token)
        .then(() => {
          void this.syncPosts(timelineName);
        })
    );
  }

  isMemberOf(username: string, timeline: TimelineInfo): boolean {
    return timeline.members.findIndex((m) => m.username == username) >= 0;
  }

  hasReadPermission(
    user: UserAuthInfo | null | undefined,
    timeline: TimelineInfo
  ): boolean {
    if (user != null && user.administrator) return true;

    const { visibility } = timeline;
    if (visibility === 'Public') {
      return true;
    } else if (visibility === 'Register') {
      if (user != null) return true;
    } else if (visibility === 'Private') {
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
    user: UserAuthInfo | null | undefined,
    timeline: TimelineInfo
  ): boolean {
    if (user != null && user.administrator) return true;

    return (
      user != null &&
      (timeline.owner.username === user.username ||
        this.isMemberOf(user.username, timeline))
    );
  }

  hasManagePermission(
    user: UserAuthInfo | null | undefined,
    timeline: TimelineInfo
  ): boolean {
    if (user != null && user.administrator) return true;

    return user != null && user.username == timeline.owner.username;
  }

  hasModifyPostPermission(
    user: UserAuthInfo | null | undefined,
    timeline: TimelineInfo,
    post: TimelinePostInfo
  ): boolean {
    if (user != null && user.administrator) return true;

    return (
      user != null &&
      (user.username === timeline.owner.username ||
        user.username === post.author.username)
    );
  }
}

export const timelineService = new TimelineService();

const timelineNameReg = XRegExp('^[-_\\p{L}]*$', 'u');

export function validateTimelineName(name: string): boolean {
  return timelineNameReg.test(name);
}

export function useTimelineInfo(
  timelineName: string
): TimelineWithSyncState | undefined {
  const [state, setState] = React.useState<TimelineWithSyncState | undefined>(
    undefined
  );
  React.useEffect(() => {
    const subscription = timelineService.timelineHub.subscribe(
      timelineName,
      (data) => {
        setState(data);
      }
    );
    return () => {
      subscription.unsubscribe();
    };
  }, [timelineName]);
  return state;
}

export function usePostList(
  timelineName: string | null | undefined
): TimelinePostsWithSyncState | undefined {
  const [state, setState] = React.useState<
    TimelinePostsWithSyncState | undefined
  >(undefined);
  React.useEffect(() => {
    if (timelineName == null) {
      setState(undefined);
      return;
    }

    const subscription = timelineService.postsHub.subscribe(
      timelineName,
      (data) => {
        setState(data);
      }
    );
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
        key.startsWith('timeline.') && (key.match(/\./g) ?? []).length === 1
    )
    .map((key) => key.substr('timeline.'.length));
}
