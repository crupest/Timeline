import React from 'react';
import XRegExp from 'xregexp';
import { Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';
import { pull } from 'lodash';

import { convertError } from '../utilities/rxjs';

import { BlobWithUrl, dataStorage } from './common';
import { SubscriptionHub, ISubscriptionHub } from './SubscriptionHub';

import { UserAuthInfo, checkLogin, userService } from './user';

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
  HttpTimelinePostContent,
  HttpTimelinePostTextContent,
  HttpTimelinePostImageContent,
  getHttpTimelineClient,
  HttpTimelineNotExistError,
  HttpTimelineNameConflictError,
  HttpTimelineGenericPostInfo,
} from '../http/timeline';
import { BlobWithEtag, NotModified } from '../http/common';

export type TimelineInfo = HttpTimelineInfo;
export type TimelineChangePropertyRequest = HttpTimelinePatchRequest;
export type TimelineCreatePostRequest = HttpTimelinePostPostRequest;
export type TimelineCreatePostContent = HttpTimelinePostPostRequestContent;
export type TimelineCreatePostTextContent = HttpTimelinePostPostRequestTextContent;
export type TimelineCreatePostImageContent = HttpTimelinePostPostRequestImageContent;

export interface TimelinePostInfo extends HttpTimelinePostInfo {
  timelineName: string;
}

export type TimelinePostContent = HttpTimelinePostContent;
export type TimelinePostTextContent = HttpTimelinePostTextContent;
export type TimelinePostImageContent = HttpTimelinePostImageContent;

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

export interface PostKey {
  timelineName: string;
  postId: number;
}

export interface TimelinePostListState {
  state:
    | 'loading' // Loading posts from cache. `posts` is empty array.
    | 'syncing' // Cache loaded and syncing now.
    | 'synced' // Sync succeeded.
    | 'offline'; // Sync failed and use cache.
  posts: TimelinePostInfo[];
}

interface PostListInfo {
  idList: number[];
  lastUpdated: string;
}

export class TimelineService {
  // TODO: Remove this! This is currently only used to avoid multiple fetch of timeline. Because post list need to use the timeline id and call this method. But after timeline is also saved locally, this should be removed.
  private timelineCache = new Map<string, Promise<TimelineInfo>>();

  getTimeline(timelineName: string): Observable<TimelineInfo> {
    const cache = this.timelineCache.get(timelineName);
    let promise: Promise<TimelineInfo>;
    if (cache == null) {
      promise = getHttpTimelineClient().getTimeline(timelineName);
      this.timelineCache.set(timelineName, promise);
    } else {
      promise = cache;
    }

    return from(promise).pipe(
      convertError(HttpTimelineNotExistError, TimelineNotExistError)
    );
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
      getHttpTimelineClient().patchTimeline(timelineName, req, user.token)
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
      getHttpTimelineClient().memberPut(timelineName, username, user.token)
    );
  }

  removeMember(timelineName: string, username: string): Observable<unknown> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient().memberDelete(timelineName, username, user.token)
    );
  }

  getPosts(timelineName: string): Observable<TimelinePostInfo[]> {
    const token = userService.currentUser?.token;
    return from(getHttpTimelineClient().listPost(timelineName, token)).pipe(
      map((posts) => {
        return posts.map((post) => ({
          ...post,
          timelineName,
        }));
      })
    );
  }

  // post list storage structure:
  // each timeline has a PostListInfo saved with key created by getPostListInfoKey
  // each post of a timeline has a HttpTimelinePostInfo with key created by getPostKey
  // each post with data has BlobWithEtag with key created by getPostDataKey

  private getPostListInfoKey(timelineUniqueId: string): string {
    return `timeline.${timelineUniqueId}.postListInfo`;
  }

  private getPostKey(timelineUniqueId: string, id: number): string {
    return `timeline.${timelineUniqueId}.post.${id}`;
  }

  private getPostDataKey(timelineUniqueId: string, id: number): string {
    return `timeline.${timelineUniqueId}.post.${id}.data`;
  }

  private async getCachedPostList(
    timelineName: string
  ): Promise<TimelinePostInfo[]> {
    const timeline = await this.getTimeline(timelineName).toPromise();
    const postListInfo = await dataStorage.getItem<PostListInfo | null>(
      this.getPostListInfoKey(timeline.uniqueId)
    );
    if (postListInfo == null) {
      return [];
    } else {
      return (
        await Promise.all(
          postListInfo.idList.map((postId) =>
            dataStorage.getItem<HttpTimelinePostInfo>(
              this.getPostKey(timeline.uniqueId, postId)
            )
          )
        )
      ).map((post) => ({ ...post, timelineName }));
    }
  }

  async syncPostList(timelineName: string): Promise<TimelinePostInfo[]> {
    const timeline = await this.getTimeline(timelineName).toPromise();
    const postListInfoKey = this.getPostListInfoKey(timeline.uniqueId);
    const postListInfo = await dataStorage.getItem<PostListInfo | null>(
      postListInfoKey
    );

    const now = new Date();
    let posts: TimelinePostInfo[];
    if (postListInfo == null) {
      let httpPosts: HttpTimelinePostInfo[];
      try {
        httpPosts = await getHttpTimelineClient().listPost(
          timelineName,
          userService.currentUser?.token
        );
      } catch (e) {
        this._postListSubscriptionHub.update(timelineName, (_, old) =>
          Promise.resolve({
            state: 'offline',
            posts: old.posts,
          })
        );
        throw e;
      }

      await dataStorage.setItem<PostListInfo>(postListInfoKey, {
        idList: httpPosts.map((post) => post.id),
        lastUpdated: now.toISOString(),
      });

      for (const post of httpPosts) {
        await dataStorage.setItem<HttpTimelinePostInfo>(
          this.getPostKey(timeline.uniqueId, post.id),
          post
        );
      }

      posts = httpPosts.map((post) => ({
        ...post,
        timelineName,
      }));
    } else {
      let httpPosts: HttpTimelineGenericPostInfo[];
      try {
        httpPosts = await getHttpTimelineClient().listPost(
          timelineName,
          userService.currentUser?.token,
          {
            modifiedSince: new Date(postListInfo.lastUpdated),
            includeDeleted: true,
          }
        );
      } catch (e) {
        this._postListSubscriptionHub.update(timelineName, (_, old) =>
          Promise.resolve({
            state: 'offline',
            posts: old.posts,
          })
        );
        throw e;
      }

      const newPosts: HttpTimelinePostInfo[] = [];

      for (const post of httpPosts) {
        if (post.deleted) {
          pull(postListInfo.idList, post.id);
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
          newPosts.push(post);
        }
      }

      const oldIdList = postListInfo.idList;

      postListInfo.idList = [...oldIdList, ...newPosts.map((post) => post.id)];
      postListInfo.lastUpdated = now.toISOString();
      await dataStorage.setItem<PostListInfo>(postListInfoKey, postListInfo);

      posts = [
        ...(await Promise.all(
          oldIdList.map((postId) =>
            dataStorage.getItem<HttpTimelinePostInfo>(
              this.getPostKey(timeline.uniqueId, postId)
            )
          )
        )),
        ...newPosts,
      ].map((post) => ({ ...post, timelineName }));
    }

    this._postListSubscriptionHub.update(timelineName, () =>
      Promise.resolve({
        state: 'synced',
        posts,
      })
    );

    return posts;
  }

  private _postListSubscriptionHub = new SubscriptionHub<
    string,
    TimelinePostListState
  >(
    (key) => key,
    () => ({
      state: 'loading',
      posts: [],
    }),
    async (key) => {
      const state: TimelinePostListState = {
        state: 'syncing',
        posts: await this.getCachedPostList(key),
      };
      void this.syncPostList(key);
      return state;
    }
  );

  get postListSubscriptionHub(): ISubscriptionHub<
    string,
    TimelinePostListState
  > {
    return this._postListSubscriptionHub;
  }

  private async getCachePostData(
    timelineName: string,
    postId: number
  ): Promise<Blob | null> {
    const timeline = await this.getTimeline(timelineName).toPromise();
    const cache = await dataStorage.getItem<BlobWithEtag | null>(
      this.getPostDataKey(timeline.uniqueId, postId)
    );
    if (cache == null) {
      return null;
    } else {
      return cache.data;
    }
  }

  private async syncCachePostData(
    timelineName: string,
    postId: number
  ): Promise<Blob | null> {
    const timeline = await this.getTimeline(timelineName).toPromise();
    const dataKey = this.getPostDataKey(timeline.uniqueId, postId);
    const cache = await dataStorage.getItem<BlobWithEtag | null>(dataKey);

    if (cache == null) {
      const dataWithEtag = await getHttpTimelineClient().getPostData(
        timelineName,
        postId,
        userService.currentUser?.token
      );
      await dataStorage.setItem<BlobWithEtag>(dataKey, dataWithEtag);
      this._postDataSubscriptionHub.update(
        {
          postId,
          timelineName,
        },
        () =>
          Promise.resolve({
            blob: dataWithEtag.data,
            url: URL.createObjectURL(dataWithEtag.data),
          })
      );
      return dataWithEtag.data;
    } else {
      const res = await getHttpTimelineClient().getPostData(
        timelineName,
        postId,
        userService.currentUser?.token,
        cache.etag
      );
      if (res instanceof NotModified) {
        return cache.data;
      } else {
        await dataStorage.setItem<BlobWithEtag>(dataKey, res);
        this._postDataSubscriptionHub.update(
          {
            postId,
            timelineName,
          },
          () =>
            Promise.resolve({
              blob: res.data,
              url: URL.createObjectURL(res.data),
            })
        );
        return res.data;
      }
    }
  }

  private _postDataSubscriptionHub = new SubscriptionHub<
    PostKey,
    BlobWithUrl | null
  >(
    (key) => `${key.timelineName}/${key.postId}`,
    () => null,
    async (key) => {
      const blob = await this.getCachePostData(key.timelineName, key.postId);
      const result =
        blob == null
          ? null
          : {
              blob,
              url: URL.createObjectURL(blob),
            };
      void this.syncCachePostData(key.timelineName, key.postId);
      return result;
    },
    (_key, data) => {
      if (data != null) URL.revokeObjectURL(data.url);
    }
  );

  get postDataHub(): ISubscriptionHub<PostKey, BlobWithUrl | null> {
    return this._postDataSubscriptionHub;
  }

  createPost(
    timelineName: string,
    request: TimelineCreatePostRequest
  ): Observable<TimelinePostInfo> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient().postPost(timelineName, request, user.token)
    ).pipe(map((post) => ({ ...post, timelineName })));
  }

  deletePost(timelineName: string, postId: number): Observable<unknown> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient().deletePost(timelineName, postId, user.token)
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
      if (user != null && this.isMemberOf(user.username, timeline)) {
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

export function usePostDataUrl(
  enable: boolean,
  timelineName: string,
  postId: number
): string | undefined {
  const [url, setUrl] = React.useState<string | undefined>(undefined);
  React.useEffect(() => {
    if (!enable) {
      setUrl(undefined);
      return;
    }

    const subscription = timelineService.postDataHub.subscribe(
      {
        timelineName,
        postId,
      },
      (data) => {
        setUrl(data?.url);
      }
    );
    return () => {
      subscription.unsubscribe();
    };
  }, [timelineName, postId, enable]);
  return url;
}
