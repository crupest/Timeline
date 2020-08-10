import React from 'react';
import XRegExp from 'xregexp';
import { Observable, from, combineLatest, of } from 'rxjs';
import { map, switchMap, startWith } from 'rxjs/operators';
import { uniqBy } from 'lodash';

import { convertError } from '../utilities/rxjs';

import { dataStorage, throwIfNotNetworkError, BlobOrStatus } from './common';
import { DataHub, WithSyncStatus } from './DataHub';

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
import { BlobWithEtag, NotModified, HttpForbiddenError } from '../http/common';
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
  data: BlobOrStatus;
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

export type TimelineWithSyncStatus = WithSyncStatus<
  | {
      type: 'cache';
      timeline: TimelineInfo;
    }
  | {
      type: 'offline' | 'synced';
      timeline: TimelineInfo | null;
    }
>;

export type TimelinePostsWithSyncState = WithSyncStatus<{
  type:
    | 'cache'
    | 'offline' // Sync failed and use cache.
    | 'synced' // Sync succeeded.
    | 'forbid' // The list is forbidden to see.
    | 'notexist'; // The timeline does not exist.
  posts: TimelinePostInfo[];
}>;

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

  private saveTimeline(
    timelineName: string,
    data: TimelineData
  ): Promise<void> {
    return dataStorage
      .setItem<TimelineData>(`timeline.${timelineName}`, data)
      .then();
  }

  private convertHttpTimelineToData(timeline: HttpTimelineInfo): TimelineData {
    return {
      ...timeline,
      owner: timeline.owner.username,
      members: timeline.members.map((m) => m.username),
    };
  }

  private async syncTimeline(timelineName: string): Promise<void> {
    const line = this._timelineHub.getLineOrCreateWithoutSetup(timelineName);
    if (line.isSyncing) return;

    if (line.value == undefined) {
      const cache = await this.getCachedTimeline(timelineName);
      if (cache != null) {
        line.next({ type: 'cache', timeline: cache });
      }
    }

    try {
      const httpTimeline = await getHttpTimelineClient().getTimeline(
        timelineName
      );

      [httpTimeline.owner, ...httpTimeline.members].forEach(
        (user) => void userInfoService.saveUser(user)
      );

      const timeline = this.convertHttpTimelineToData(httpTimeline);
      await this.saveTimeline(timelineName, timeline);
      line.endSyncAndNext({ type: 'synced', timeline });
    } catch (e) {
      if (e instanceof HttpTimelineNotExistError) {
        line.endSyncAndNext({ type: 'synced', timeline: null });
      } else {
        const cache = await this.getCachedTimeline(timelineName);
        if (cache == null) {
          line.endSyncAndNext({ type: 'offline', timeline: null });
        } else {
          line.endSyncAndNext({ type: 'offline', timeline: cache });
        }
        throwIfNotNetworkError(e);
      }
    }
  }

  private _timelineHub = new DataHub<
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
    setup: (key) => {
      void this.syncTimeline(key);
    },
  });

  getTimeline$(timelineName: string): Observable<TimelineWithSyncStatus> {
    return this._timelineHub.getDataWithSyncStatusObservable(timelineName).pipe(
      switchMap((state) => {
        const { timeline } = state;
        if (timeline != null) {
          return combineLatest(
            [timeline.owner, ...timeline.members].map((u) =>
              userInfoService.getUser$(u)
            )
          ).pipe(
            map((users) => {
              return {
                ...state,
                timeline: {
                  ...timeline,
                  owner: users[0],
                  members: users.slice(1),
                },
              };
            })
          );
        } else {
          return of(state as TimelineWithSyncStatus);
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

  private getCachedPosts(
    timelineName: string
  ): Promise<TimelinePostData[] | null> {
    return dataStorage.getItem<TimelinePostData[] | null>(
      `timeline.${timelineName}.posts`
    );
  }

  private savePosts(
    timelineName: string,
    data: TimelinePostData[]
  ): Promise<void> {
    return dataStorage
      .setItem<TimelinePostData[]>(`timeline.${timelineName}.posts`, data)
      .then();
  }

  private async syncPosts(timelineName: string): Promise<void> {
    const line = this._postsHub.getLineOrCreateWithoutSetup(timelineName);
    if (line.isSyncing) return;
    line.beginSync();

    if (line.value == null) {
      const cache = await this.getCachedPosts(timelineName);
      if (cache != null) {
        line.next({ type: 'cache', posts: cache });
      }
    }

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
      await this.savePosts(timelineName, posts);
      line.endSyncAndNext({ type: 'synced', posts });
    } catch (e) {
      if (e instanceof HttpTimelineNotExistError) {
        line.endSyncAndNext({ type: 'notexist', posts: [] });
      } else if (e instanceof HttpForbiddenError) {
        line.endSyncAndNext({ type: 'forbid', posts: [] });
      } else {
        const cache = await this.getCachedPosts(timelineName);
        if (cache == null) {
          line.endSyncAndNext({ type: 'offline', posts: [] });
        } else {
          line.endSyncAndNext({ type: 'offline', posts: cache });
        }
        throwIfNotNetworkError(e);
      }
    }
  }

  private _postsHub = new DataHub<
    string,
    {
      type: 'cache' | 'offline' | 'synced' | 'forbid' | 'notexist';
      posts: TimelinePostData[];
    }
  >({
    setup: (key) => {
      void this.syncPosts(key);
    },
  });

  getPosts$(timelineName: string): Observable<TimelinePostsWithSyncState> {
    return this._postsHub.getDataWithSyncStatusObservable(timelineName).pipe(
      switchMap((state) => {
        if (state.posts.length === 0) {
          return of({
            ...state,
            posts: [],
          });
        }

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
              ...state,
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

  private getCachedPostData(key: {
    timelineName: string;
    postId: number;
  }): Promise<BlobWithEtag | null> {
    return dataStorage.getItem<BlobWithEtag | null>(
      `timeline.${key.timelineName}.post.${key.postId}.data`
    );
  }

  private savePostData(
    key: {
      timelineName: string;
      postId: number;
    },
    data: BlobWithEtag
  ): Promise<void> {
    return dataStorage
      .setItem<BlobWithEtag>(
        `timeline.${key.timelineName}.post.${key.postId}.data`,
        data
      )
      .then();
  }

  private async syncPostData(key: {
    timelineName: string;
    postId: number;
  }): Promise<void> {
    const line = this._postDataHub.getLineOrCreateWithoutSetup(key);
    if (line.isSyncing) return;
    line.beginSync();

    const cache = await this.getCachedPostData(key);
    if (line.value == null) {
      if (cache != null) {
        line.next({ type: 'cache', data: cache.data });
      }
    }

    if (cache == null) {
      try {
        const res = await getHttpTimelineClient().getPostData(
          key.timelineName,
          key.postId
        );
        await this.savePostData(key, res);
        line.endSyncAndNext({ data: res.data, type: 'synced' });
      } catch (e) {
        line.endSyncAndNext({ type: 'offline' });
        throwIfNotNetworkError(e);
      }
    } else {
      try {
        const res = await getHttpTimelineClient().getPostData(
          key.timelineName,
          key.postId,
          cache.etag
        );
        if (res instanceof NotModified) {
          line.endSyncAndNext({ data: cache.data, type: 'synced' });
        } else {
          await this.savePostData(key, res);
          line.endSyncAndNext({ data: res.data, type: 'synced' });
        }
      } catch (e) {
        line.endSyncAndNext({ data: cache.data, type: 'offline' });
        throwIfNotNetworkError(e);
      }
    }
  }

  private _postDataHub = new DataHub<
    { timelineName: string; postId: number },
    | { data: Blob; type: 'cache' | 'synced' | 'offline' }
    | { data?: undefined; type: 'notexist' | 'offline' }
  >({
    keyToString: (key) => `${key.timelineName}.${key.postId}`,
    setup: (key) => {
      void this.syncPostData(key);
    },
  });

  getPostData$(timelineName: string, postId: number): Observable<BlobOrStatus> {
    return this._postDataHub.getObservable({ timelineName, postId }).pipe(
      map((state): BlobOrStatus => state.data ?? 'error'),
      startWith('loading')
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
