import React from 'react';
import XRegExp from 'xregexp';
import { Observable, from, combineLatest, of } from 'rxjs';
import { map, switchMap, filter } from 'rxjs/operators';
import { uniqBy } from 'lodash';

import { convertError } from '../utilities/rxjs';

import { dataStorage } from './common';
import { SubscriptionHub } from './SubscriptionHub';
import syncStatusHub from './SyncStatusHub';

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

export type TimelineWithSyncStatus =
  | {
      type: 'cache';
      timeline: TimelineInfo;
    }
  | {
      type: 'offline' | 'synced';
      timeline: TimelineInfo | null;
    }
  | {
      type: 'notexist';
      timeline?: undefined;
    };

export interface TimelinePostsWithSyncState {
  type:
    | 'cache'
    | 'offline' // Sync failed and use cache.
    | 'synced' // Sync succeeded.
    | 'forbid' // The list is forbidden to see.
    | 'notexist'; // The timeline does not exist.
  posts: TimelinePostInfo[];
}

type TimelineData = Omit<HttpTimelineInfo, 'owner' | 'members'> & {
  owner: string;
  members: string[];
};

type TimelinePostData = Omit<HttpTimelinePostInfo, 'author'> & {
  author: string;
};

export class TimelineService {
  private getCachedTimeline(
    timelineName: string
  ): Promise<TimelineData | null> {
    return dataStorage.getItem<TimelineData | null>(`timeline.${timelineName}`);
  }

  private convertHttpTimelineToData(timeline: HttpTimelineInfo): TimelineData {
    return {
      ...timeline,
      owner: timeline.owner.username,
      members: timeline.members.map((m) => m.username),
    };
  }

  private async syncTimeline(timelineName: string): Promise<void> {
    const syncStatusKey = `timeline.${timelineName}`;
    if (syncStatusHub.get(syncStatusKey)) return;
    syncStatusHub.begin(syncStatusKey);

    try {
      const httpTimeline = await getHttpTimelineClient().getTimeline(
        timelineName
      );

      [httpTimeline.owner, ...httpTimeline.members].forEach(
        (user) => void userInfoService.saveUser(user)
      );

      const timeline = this.convertHttpTimelineToData(httpTimeline);
      await dataStorage.setItem<TimelineData>(
        `timeline.${timelineName}`,
        timeline
      );

      syncStatusHub.end(syncStatusKey);
      this._timelineHub
        .getLine(timelineName)
        ?.next({ type: 'synced', timeline });
    } catch (e) {
      syncStatusHub.end(syncStatusKey);
      if (e instanceof HttpTimelineNotExistError) {
        this._timelineHub
          .getLine(timelineName)
          ?.next({ type: 'synced', timeline: null });
      } else if (e instanceof HttpNetworkError) {
        const cache = await this.getCachedTimeline(timelineName);
        if (cache == null)
          this._timelineHub
            .getLine(timelineName)
            ?.next({ type: 'offline', timeline: null });
        else
          this._timelineHub
            .getLine(timelineName)
            ?.next({ type: 'offline', timeline: cache });
      } else {
        throw e;
      }
    }
  }

  private _timelineHub = new SubscriptionHub<
    string,
    | {
        type: 'cache';
        timeline: TimelineData;
      }
    | {
        type: 'offline' | 'synced';
        timeline: TimelineData | null;
      }
  >({
    setup: (key, line) => {
      void this.getCachedTimeline(key).then((timeline) => {
        if (timeline != null) {
          line.next({ type: 'cache', timeline });
        }
        return this.syncTimeline(key);
      });
    },
  });

  getTimeline$(timelineName: string): Observable<TimelineWithSyncStatus> {
    return this._timelineHub.getObservable(timelineName).pipe(
      switchMap((state) => {
        if (state.timeline != null) {
          return combineLatest(
            [state.timeline.owner, ...state.timeline.members].map((u) =>
              userInfoService.getUser$(u)
            )
          ).pipe(
            map((users) => {
              return {
                type: 'cache',
                timeline: {
                  ...state.timeline,
                  owner: users[0],
                  members: users.slice(1),
                },
              } as TimelineWithSyncStatus;
            })
          );
        } else {
          return [
            {
              ...state,
            } as TimelineWithSyncStatus,
          ];
        }
      })
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
          void this.syncTimeline(timelineName);
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

  private convertHttpPostToData(post: HttpTimelinePostInfo): TimelinePostData {
    return {
      ...post,
      author: post.author.username,
    };
  }

  private convertHttpPostToDataList(
    posts: HttpTimelinePostInfo[]
  ): TimelinePostData[] {
    return posts.map((post) => this.convertHttpPostToData(post));
  }

  private async getCachedPosts(
    timelineName: string
  ): Promise<TimelinePostData[]> {
    const posts = await dataStorage.getItem<TimelinePostData[] | null>(
      `timeline.${timelineName}.posts`
    );
    if (posts == null) return [];
    return posts;
  }

  private async syncPosts(timelineName: string): Promise<void> {
    const syncStatusKey = `timeline.posts.${timelineName}`;

    const dataKey = `timeline.${timelineName}.posts`;
    if (syncStatusHub.get(syncStatusKey)) return;
    syncStatusHub.begin(syncStatusKey);

    try {
      const httpPosts = await getHttpTimelineClient().listPost(
        timelineName,
        userService.currentUser?.token
      );

      uniqBy(
        httpPosts.map((post) => post.author),
        'username'
      ).forEach((user) => void userInfoService.saveUser(user));

      const posts = this.convertHttpPostToDataList(httpPosts);
      await dataStorage.setItem<TimelinePostData[]>(dataKey, posts);

      syncStatusHub.end(syncStatusKey);
      this._postsHub.getLine(timelineName)?.next({ type: 'synced', posts });
    } catch (e) {
      syncStatusHub.end(syncStatusKey);
      if (e instanceof HttpTimelineNotExistError) {
        this._postsHub
          .getLine(timelineName)
          ?.next({ type: 'notexist', posts: [] });
      } else if (e instanceof HttpForbiddenError) {
        this._postsHub
          .getLine(timelineName)
          ?.next({ type: 'forbid', posts: [] });
      } else if (e instanceof HttpNetworkError) {
        const cache = await this.getCachedPosts(timelineName);
        if (cache == null)
          this._postsHub
            .getLine(timelineName)
            ?.next({ type: 'offline', posts: [] });
        else
          this._postsHub
            .getLine(timelineName)
            ?.next({ type: 'offline', posts: cache });
      } else {
        throw e;
      }
    }
  }

  private _postsHub = new SubscriptionHub<
    string,
    {
      type: 'cache' | 'offline' | 'synced' | 'forbid' | 'notexist';
      posts: TimelinePostData[];
    }
  >({
    setup: (key, line) => {
      void this.getCachedPosts(key).then((posts) => {
        if (posts != null) {
          line.next({ type: 'cache', posts });
        }
        return this.syncPosts(key);
      });
    },
  });

  getPosts$(timelineName: string): Observable<TimelinePostsWithSyncState> {
    return this._postsHub.getObservable(timelineName).pipe(
      switchMap((state) => {
        return combineLatest([
          combineLatest(
            state.posts.map((post) => userInfoService.getUser$(post.author))
          ),
          combineLatest(
            state.posts.map((post) => {
              if (post.content.type === 'image') {
                return this.getPostData$(timelineName, post.id);
              } else {
                return of(null);
              }
            })
          ),
        ]).pipe(
          map(([authors, datas]) => {
            return {
              type: state.type,
              posts: state.posts.map((post, i) => {
                const { content } = post;

                return {
                  ...post,
                  author: authors[i],
                  content: (() => {
                    if (content.type === 'text') return content;
                    else
                      return {
                        type: 'image',
                        data: datas[i],
                      } as TimelinePostImageContent;
                  })(),
                };
              }),
            };
          })
        );
      })
    );
  }

  private getCachedPostData(
    timelineName: string,
    postId: number
  ): Promise<Blob | null> {
    return dataStorage
      .getItem<BlobWithEtag | null>(
        `timeline.${timelineName}.post.${postId}.data`
      )
      .then((data) => data?.data ?? null);
  }

  private async syncPostData(
    timelineName: string,
    postId: number
  ): Promise<void> {
    const syncStatusKey = `user.timeline.${timelineName}.post.data.${postId}`;
    if (syncStatusHub.get(syncStatusKey)) return;
    syncStatusHub.begin(syncStatusKey);

    const dataKey = `timeline.${timelineName}.post.${postId}.data`;

    const cache = await dataStorage.getItem<BlobWithEtag | null>(dataKey);
    if (cache == null) {
      try {
        const data = await getHttpTimelineClient().getPostData(
          timelineName,
          postId
        );
        await dataStorage.setItem<BlobWithEtag>(dataKey, data);
        syncStatusHub.end(syncStatusKey);
        this._postDataHub
          .getLine({ timelineName, postId })
          ?.next({ data: data.data, type: 'synced' });
      } catch (e) {
        syncStatusHub.end(syncStatusKey);
        this._postDataHub
          .getLine({ timelineName, postId })
          ?.next({ type: 'offline' });
        if (!(e instanceof HttpNetworkError)) {
          throw e;
        }
      }
    } else {
      try {
        const res = await getHttpTimelineClient().getPostData(
          timelineName,
          postId,
          cache.etag
        );
        if (res instanceof NotModified) {
          syncStatusHub.end(syncStatusKey);
          this._postDataHub
            .getLine({ timelineName, postId })
            ?.next({ data: cache.data, type: 'synced' });
        } else {
          const avatar = res;
          await dataStorage.setItem<BlobWithEtag>(dataKey, avatar);
          syncStatusHub.end(syncStatusKey);
          this._postDataHub
            .getLine({ timelineName, postId })
            ?.next({ data: avatar.data, type: 'synced' });
        }
      } catch (e) {
        syncStatusHub.end(syncStatusKey);
        this._postDataHub
          .getLine({ timelineName, postId })
          ?.next({ data: cache.data, type: 'offline' });
        if (!(e instanceof HttpNetworkError)) {
          throw e;
        }
      }
    }
  }

  private _postDataHub = new SubscriptionHub<
    { timelineName: string; postId: number },
    | { data: Blob; type: 'cache' | 'synced' | 'offline' }
    | { data?: undefined; type: 'notexist' | 'offline' }
  >({
    keyToString: (key) => `${key.timelineName}.${key.postId}`,
    setup: (key, line) => {
      void this.getCachedPostData(key.timelineName, key.postId).then((data) => {
        if (data != null) {
          line.next({ data: data, type: 'cache' });
        }
        return this.syncPostData(key.timelineName, key.postId);
      });
    },
  });

  getPostData$(timelineName: string, postId: number): Observable<Blob> {
    return this._postDataHub.getObservable({ timelineName, postId }).pipe(
      map((state) => state.data),
      filter((blob): blob is Blob => blob != null)
    );
  }

  createPost(
    timelineName: string,
    request: TimelineCreatePostRequest
  ): Observable<unknown> {
    const user = checkLogin();
    return from(
      getHttpTimelineClient()
        .postPost(timelineName, request, user.token)
        .then(() => {
          void this.syncPosts(timelineName);
        })
    );
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
): TimelineWithSyncStatus | undefined {
  const [state, setState] = React.useState<TimelineWithSyncStatus | undefined>(
    undefined
  );
  React.useEffect(() => {
    const subscription = timelineService
      .getTimeline$(timelineName)
      .subscribe((data) => {
        setState(data);
      });
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

    const subscription = timelineService
      .getPosts$(timelineName)
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
        key.startsWith('timeline.') && (key.match(/\./g) ?? []).length === 1
    )
    .map((key) => key.substr('timeline.'.length));
}
