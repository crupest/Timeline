import React from 'react';
import XRegExp from 'xregexp';
import { Observable, from } from 'rxjs';
import { map } from 'rxjs/operators';

import { UserAuthInfo, checkLogin, userService } from './user';

import { BlobWithUrl } from './common';
import { SubscriptionHub, ISubscriptionHub } from './SubscriptionHub';

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
} from '../http/timeline';
import { convertError } from '../utilities/rxjs';

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

export class TimelineService {
  getTimeline(timelineName: string): Observable<TimelineInfo> {
    return from(getHttpTimelineClient().getTimeline(timelineName)).pipe(
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
      // TODO: Implement cache
      return {
        state: 'synced',
        posts: (
          await getHttpTimelineClient().listPost(
            key,
            userService.currentUser?.token
          )
        ).map((post) => ({
          ...post,
          timelineName: key,
        })),
      };
    }
  );

  get postListSubscriptionHub(): ISubscriptionHub<
    string,
    TimelinePostListState
  > {
    return this._postListSubscriptionHub;
  }

  private _postDataSubscriptionHub = new SubscriptionHub<
    PostKey,
    BlobWithUrl | null
  >(
    (key) => `${key.timelineName}/${key.postId}`,
    () => null,
    async (key) => {
      const blob = (
        await getHttpTimelineClient().getPostData(
          key.timelineName,
          key.postId,
          userService.currentUser?.token
        )
      ).data;
      const url = URL.createObjectURL(blob);
      return {
        blob,
        url,
      };
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
