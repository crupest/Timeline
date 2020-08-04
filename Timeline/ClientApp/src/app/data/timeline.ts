import React from 'react';
import XRegExp from 'xregexp';
import { Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';
import { pull } from 'lodash';

import { convertError } from '../utilities/rxjs';

import { dataStorage } from './common';
import { SubscriptionHub, ISubscriptionHub, NoValue } from './SubscriptionHub';

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
      syncState:
        | 'offline' // Sync failed and use cache. Null timeline means no cache.
        | 'synced'; // Sync succeeded. Null timeline means the timeline does not exist.
      timeline: TimelineInfo | null;
    }
  | {
      syncState: 'new'; // This is a new timeline different from cached one.
      timeline: TimelineInfo;
    };

export interface TimelinePostsWithSyncState {
  state:
    | 'forbid' // The list is forbidden to see.
    | 'synced' // Sync succeeded.
    | 'offline'; // Sync failed and use cache.
  posts: TimelinePostInfo[];
}

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

  private async fetchAndCacheTimeline(
    timelineName: string
  ): Promise<
    | { timeline: TimelineInfo; type: 'new' | 'cache' | 'synced' }
    | 'offline'
    | 'notexist'
  > {
    const cache = await dataStorage.getItem<TimelineCache | null>(timelineName);
    const key = this.getTimelineKey(timelineName);

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

  private _timelineSubscriptionHub = new SubscriptionHub<
    string,
    TimelineWithSyncState
  >({
    setup: (key, next) => {
      void this.fetchAndCacheTimeline(key).then((result) => {
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
      });
    },
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
          this._timelineSubscriptionHub.update(timelineName, {
            syncState: 'synced',
            timeline,
          });
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
          userInfoService.getUserInfo(username).subscribe((newUser) => {
            this._timelineSubscriptionHub.updateWithOld(timelineName, (old) => {
              if (old instanceof NoValue || old.timeline == null)
                throw new Error('Timeline not loaded.');

              return {
                ...old,
                timeline: {
                  ...old.timeline,
                  members: [...old.timeline.members, newUser],
                },
              };
            });
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
          this._timelineSubscriptionHub.updateWithOld(timelineName, (old) => {
            if (old instanceof NoValue || old.timeline == null)
              throw new Error('Timeline not loaded.');

            return {
              ...old,
              timeline: {
                ...old.timeline,
                members: old.timeline.members.filter(
                  (u) => u.username !== username
                ),
              },
            };
          });
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

  async fetchAndCachePosts(
    timeline: TimelineInfo
  ): Promise<
    | { posts: TimelinePostInfo[]; type: 'synced' | 'cache' }
    | 'forbid'
    | 'offline'
  > {
    if (!this.hasReadPermission(userService.currentUser, timeline)) {
      return 'forbid';
    }

    const postsInfoKey = this.getPostsInfoKey(timeline.uniqueId);
    const postsInfo = await dataStorage.getItem<PostsInfoCache | null>(
      postsInfoKey
    );

    const convertPostList = (
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

  private _postsSubscriptionHub = new SubscriptionHub<
    string,
    TimelinePostsWithSyncState
  >({
    setup: (key, next) => {
      const sub = this.timelineHub.subscribe(key, (timelineState) => {
        if (timelineState.timeline == null) {
          if (timelineState.syncState === 'offline') {
            next({ state: 'offline', posts: [] });
          } else {
            next({ state: 'synced', posts: [] });
          }
        } else {
          void this.fetchAndCachePosts(timelineState.timeline).then(
            (result) => {
              if (result === 'forbid') {
                next({ state: 'forbid', posts: [] });
              } else if (result === 'offline') {
                next({ state: 'offline', posts: [] });
              } else if (result.type === 'synced') {
                next({ state: 'synced', posts: result.posts });
              } else {
                next({ state: 'offline', posts: result.posts });
              }
            }
          );
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
          this._postsSubscriptionHub.updateWithOld(timelineName, (old) => {
            if (old instanceof NoValue) {
              throw new Error('Posts has not been loaded.');
            }
            return {
              ...old,
              posts: [...old.posts, post],
            };
          });
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
          this._postsSubscriptionHub.updateWithOld(timelineName, (old) => {
            if (old instanceof NoValue) {
              throw new Error('Posts has not been loaded.');
            }
            return {
              ...old,
              posts: old.posts.filter((post) => post.id != postId),
            };
          });
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
