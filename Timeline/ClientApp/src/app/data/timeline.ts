import React from 'react';
import XRegExp from 'xregexp';
import { Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';
import { pull } from 'lodash';

import { convertError } from '../utilities/rxjs';

import { dataStorage } from './common';
import { queue } from './queue';
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
import { BlobWithEtag, NotModified, HttpNetworkError } from '../http/common';
import { HttpUser } from '../http/user';
import { ExcludeKey } from '../utilities/type';

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
        | 'synced'; // Sync succeeded. Null timeline means the timeline does not exist.
      timeline: TimelineInfo | null;
    }
  | {
      syncState: 'new'; // This is a new timeline different from cached one.
      timeline: TimelineInfo;
    };

export interface TimelinePostsTimelineWithSyncState {
  state:
    | 'loadcache'
    | 'syncing' // Syncing now.
    | 'offline' // Sync failed and use cache.
    | 'synced' // Sync succeeded.
    | 'forbid'; // The list is forbidden to see.
  posts: TimelinePostInfo[];
  timelineUniqueId: string;
}

export interface TimelinePostsNoTimelineWithSyncState {
  state: 'timeline-offline' | 'timeline-notexist';
  posts?: undefined;
  timelineUniqueId?: undefined;
}

export type TimelinePostsWithSyncState =
  | TimelinePostsTimelineWithSyncState
  | TimelinePostsNoTimelineWithSyncState;

type FetchAndCacheTimelineResult =
  | { timeline: TimelineInfo; type: 'new' | 'cache' | 'synced' }
  | 'offline'
  | 'notexist';

type FetchAndCachePostsResult =
  | { posts: TimelinePostInfo[]; type: 'synced' | 'cache' }
  | 'offline';

interface TimelineCache {
  timeline: TimelineInfo;
  lastUpdated: string;
}

interface PostsInfoCache {
  idList: number[];
  lastUpdated: string;
}

export class TimelineService {
  // timeline storage structure:
  // each timeline has a TimelineCache saved with key created by getTimelineKey

  private getTimelineKey(timelineName: string): string {
    return `timeline.${timelineName}`;
  }

  private getCachedTimeline(
    timelineName: string
  ): Promise<TimelineInfo | null> {
    return dataStorage
      .getItem<TimelineCache | null>(timelineName)
      .then((cache) => cache?.timeline ?? null);
  }

  private fetchAndCacheTimeline(
    timelineName: string
  ): Promise<FetchAndCacheTimelineResult> {
    return queue(`TimelineService.fetchAndCacheTimeline.${timelineName}`, () =>
      this.doFetchAndCacheTimeline(timelineName)
    );
  }

  private async doFetchAndCacheTimeline(
    timelineName: string
  ): Promise<FetchAndCacheTimelineResult> {
    const key = this.getTimelineKey(timelineName);
    const cache = await dataStorage.getItem<TimelineCache | null>(key);

    const save = (cache: TimelineCache): Promise<TimelineCache> =>
      dataStorage.setItem<TimelineCache>(key, cache);

    const now = new Date();
    if (cache == null) {
      try {
        const timeline = await getHttpTimelineClient().getTimeline(
          timelineName
        );
        await save({ timeline, lastUpdated: now.toISOString() });
        return { timeline, type: 'synced' };
      } catch (e) {
        if (e instanceof HttpTimelineNotExistError) {
          return 'notexist';
        } else if (e instanceof HttpNetworkError) {
          return 'offline';
        } else {
          throw e;
        }
      }
    } else {
      try {
        const res = await getHttpTimelineClient().getTimeline(timelineName, {
          checkUniqueId: cache.timeline.uniqueId,
          ifModifiedSince: new Date(cache.lastUpdated),
        });
        if (res instanceof NotModified) {
          const { timeline } = cache;
          await save({ timeline, lastUpdated: now.toISOString() });
          return { timeline, type: 'synced' };
        } else {
          const timeline = res;
          await save({ timeline, lastUpdated: now.toISOString() });
          if (res.uniqueId === cache.timeline.uniqueId) {
            return { timeline, type: 'synced' };
          } else {
            return { timeline, type: 'new' };
          }
        }
      } catch (e) {
        if (e instanceof HttpTimelineNotExistError) {
          await dataStorage.removeItem(key);
          return 'notexist';
        } else if (e instanceof HttpNetworkError) {
          return { timeline: cache.timeline, type: 'cache' };
        }
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
      next({ syncState: 'offline', timeline: null });
    } else if (result === 'notexist') {
      next({ syncState: 'synced', timeline: null });
    } else {
      const { type, timeline } = result;
      if (type === 'cache') {
        next({ syncState: 'offline', timeline });
      } else if (type === 'synced') {
        next({ syncState: 'synced', timeline });
      } else {
        next({ syncState: 'new', timeline });
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

  // post list storage structure:
  // each timeline has a PostsInfoCache saved with key created by getPostsInfoKey
  // each post of a timeline has a HttpTimelinePostInfo with key created by getPostKey
  // each post with data has BlobWithEtag with key created by getPostDataKey

  private getPostsInfoKey(timelineUniqueId: string): string {
    return `timeline.${timelineUniqueId}.postListInfo`;
  }

  private getPostKey(timelineUniqueId: string, id: number): string {
    return `timeline.${timelineUniqueId}.post.${id}`;
  }

  private getPostDataKey(timelineUniqueId: string, id: number): string {
    return `timeline.${timelineUniqueId}.post.${id}.data`;
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

  private async getCachedPosts(timeline: {
    name: string;
    uniqueId: string;
  }): Promise<TimelinePostInfo[]> {
    const postsInfoKey = this.getPostsInfoKey(timeline.uniqueId);
    const postsInfo = await dataStorage.getItem<PostsInfoCache | null>(
      postsInfoKey
    );

    if (postsInfo == null) return [];

    const httpPosts = await Promise.all(
      postsInfo.idList.map((postId) =>
        dataStorage.getItem<HttpTimelinePostInfo>(
          this.getPostKey(timeline.uniqueId, postId)
        )
      )
    );

    const posts = await this.convertPostList(httpPosts, (post) =>
      dataStorage
        .getItem<BlobWithEtag | null>(
          this.getPostDataKey(timeline.uniqueId, post.id)
        )
        .then((d) => d?.data)
    );

    return posts;
  }

  private fetchAndCachePosts(timeline: {
    name: string;
    uniqueId: string;
  }): Promise<FetchAndCachePostsResult> {
    return queue(
      `TimelineService.fetchAndCachePosts.${timeline.uniqueId}`,
      () => this.doFetchAndCachePosts(timeline)
    );
  }

  private async doFetchAndCachePosts(timeline: {
    name: string;
    uniqueId: string;
  }): Promise<FetchAndCachePostsResult> {
    const postsInfoKey = this.getPostsInfoKey(timeline.uniqueId);
    const postsInfo = await dataStorage.getItem<PostsInfoCache | null>(
      postsInfoKey
    );

    const convertPostList = this.convertPostList.bind(this);

    const now = new Date();
    if (postsInfo == null) {
      try {
        const token = userService.currentUser?.token;

        const httpPosts = await getHttpTimelineClient().listPost(
          timeline.name,
          token
        );

        const dataList: (BlobWithEtag | null)[] = await Promise.all(
          httpPosts.map(async (post) => {
            const { content } = post;
            if (content.type === 'image') {
              return await getHttpTimelineClient().getPostData(
                timeline.name,
                post.id,
                token
              );
            } else {
              return null;
            }
          })
        );

        await dataStorage.setItem<PostsInfoCache>(postsInfoKey, {
          idList: httpPosts.map((post) => post.id),
          lastUpdated: now.toISOString(),
        });

        for (const [i, post] of httpPosts.entries()) {
          await dataStorage.setItem<HttpTimelinePostInfo>(
            this.getPostKey(timeline.uniqueId, post.id),
            post
          );
          const data = dataList[i];
          if (data != null) {
            await dataStorage.setItem<BlobWithEtag>(
              this.getPostDataKey(timeline.uniqueId, post.id),
              data
            );
          }
        }

        const posts: TimelinePostInfo[] = await convertPostList(
          httpPosts,
          (post, i) => Promise.resolve(dataList[i]?.data)
        );

        return { posts, type: 'synced' };
      } catch (e) {
        if (e instanceof HttpNetworkError) {
          return 'offline';
        } else {
          throw e;
        }
      }
    } else {
      try {
        const token = userService.currentUser?.token;
        const httpPosts = await getHttpTimelineClient().listPost(
          timeline.name,
          token,
          {
            modifiedSince: new Date(postsInfo.lastUpdated),
            includeDeleted: true,
          }
        );

        const dataList: (BlobWithEtag | null)[] = await Promise.all(
          httpPosts.map(async (post) => {
            if (post.deleted) return null;
            const { content } = post;
            if (content.type === 'image') {
              return await getHttpTimelineClient().getPostData(
                timeline.name,
                post.id,
                token
              );
            } else {
              return null;
            }
          })
        );

        const newPosts: HttpTimelinePostInfo[] = [];
        const newPostDataList: (BlobWithEtag | null)[] = [];

        for (const [i, post] of httpPosts.entries()) {
          if (post.deleted) {
            pull(postsInfo.idList, post.id);
            await dataStorage.removeItem(
              this.getPostKey(timeline.uniqueId, post.id)
            );
            await dataStorage.removeItem(
              this.getPostDataKey(timeline.uniqueId, post.id)
            );
          } else {
            await dataStorage.setItem<HttpTimelinePostInfo>(
              this.getPostKey(timeline.uniqueId, post.id),
              post
            );
            const data = dataList[i];
            if (data != null) {
              await dataStorage.setItem<BlobWithEtag>(
                this.getPostDataKey(timeline.uniqueId, post.id),
                data
              );
            }
            newPosts.push(post);
            newPostDataList.push(data);
          }
        }

        const oldIdList = postsInfo.idList;

        postsInfo.idList = [...oldIdList, ...newPosts.map((post) => post.id)];
        postsInfo.lastUpdated = now.toISOString();
        await dataStorage.setItem<PostsInfoCache>(postsInfoKey, postsInfo);

        const posts: TimelinePostInfo[] = [
          ...(await convertPostList(
            await Promise.all(
              oldIdList.map((postId) =>
                dataStorage.getItem<HttpTimelinePostInfo>(
                  this.getPostKey(timeline.uniqueId, postId)
                )
              )
            ),
            (post) =>
              dataStorage
                .getItem<BlobWithEtag | null>(
                  this.getPostDataKey(timeline.uniqueId, post.id)
                )
                .then((d) => d?.data)
          )),
          ...(await convertPostList(newPosts, (post, i) =>
            Promise.resolve(newPostDataList[i]?.data)
          )),
        ];
        return { posts, type: 'synced' };
      } catch (e) {
        if (e instanceof HttpNetworkError) {
          const httpPosts = await Promise.all(
            postsInfo.idList.map((postId) =>
              dataStorage.getItem<HttpTimelinePostInfo>(
                this.getPostKey(timeline.uniqueId, postId)
              )
            )
          );

          const posts = await convertPostList(httpPosts, (post) =>
            dataStorage
              .getItem<BlobWithEtag | null>(
                this.getPostDataKey(timeline.uniqueId, post.id)
              )
              .then((d) => d?.data)
          );

          return { posts, type: 'cache' };
        } else {
          throw e;
        }
      }
    }
  }

  private syncPosts(timelineName: string): Promise<void> {
    const line = this._postsSubscriptionHub.getLine(timelineName);
    if (line == null) return Promise.resolve();

    const { value } = line;

    if (
      value != null &&
      value.timelineUniqueId != null &&
      value.state !== 'forbid'
    ) {
      return this.syncPostsWithUniqueId({
        name: timelineName,
        uniqueId: value.timelineUniqueId,
      });
    } else {
      return Promise.resolve();
    }
  }

  private async syncPostsWithUniqueId(timeline: {
    name: string;
    uniqueId: string;
  }): Promise<void> {
    const line = this._postsSubscriptionHub.getLine(timeline.name);
    if (line == null) return;

    if (
      line.value != null &&
      line.value.timelineUniqueId == timeline.uniqueId &&
      (line.value.state === 'loadcache' || line.value.state === 'syncing')
    ) {
      return;
    }

    const next = (
      value: ExcludeKey<TimelinePostsTimelineWithSyncState, 'timelineUniqueId'>
    ): void => {
      line.next({
        ...value,
        timelineUniqueId: timeline.uniqueId,
      });
    };

    const uniqueIdChanged = (): boolean => {
      return line.value?.timelineUniqueId !== timeline.uniqueId;
    };

    if (line.value == null) {
      next({
        state: 'loadcache',
        posts: [],
      });
      const posts = await this.getCachedPosts(timeline);
      if (uniqueIdChanged()) {
        return;
      }
      next({
        state: 'syncing',
        posts,
      });
    } else {
      next({
        state: 'syncing',
        posts: line.value?.posts ?? [],
      });
    }

    const result = await this.fetchAndCachePosts(timeline);
    if (uniqueIdChanged()) {
      return;
    }

    if (result === 'offline') {
      next({ state: 'offline', posts: [] });
    } else if (result.type === 'synced') {
      next({ state: 'synced', posts: result.posts });
    } else {
      next({ state: 'offline', posts: result.posts });
    }
  }

  private _postsSubscriptionHub = new SubscriptionHub<
    string,
    TimelinePostsWithSyncState
  >({
    setup: (key, line) => {
      const sub = this.timelineHub.subscribe(key, (timelineState) => {
        if (timelineState.timeline != null) {
          if (
            !this.hasReadPermission(
              userService.currentUser,
              timelineState.timeline
            )
          ) {
            line.next({
              state: 'forbid',
              posts: [],
              timelineUniqueId: timelineState.timeline.uniqueId,
            });
          } else {
            if (
              line.value == null ||
              line.value.timelineUniqueId !== timelineState.timeline.uniqueId
            ) {
              void this.syncPostsWithUniqueId(timelineState.timeline);
            }
          }
        } else {
          if (timelineState.syncState === 'synced') {
            line.next({
              state: 'timeline-notexist',
            });
          } else if (timelineState.syncState === 'offline') {
            line.next({
              state: 'timeline-offline',
            });
          }
        }
      });
      return () => {
        sub.unsubscribe();
      };
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
