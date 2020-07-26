import React from 'react';
import { useTranslation } from 'react-i18next';
import { concat, without } from 'lodash';
import { of } from 'rxjs';
import { catchError, switchMap, map } from 'rxjs/operators';

import { ExcludeKey } from '../utilities/type';
import { pushAlert } from '../common/alert-service';
import { useUser, userInfoService, UserNotExistError } from '../data/user';
import {
  timelineService,
  TimelineInfo,
  TimelineNotExistError,
} from '../data/timeline';

import { TimelinePostInfoEx, TimelineDeleteCallback } from './Timeline';
import { TimelineMemberDialog } from './TimelineMember';
import TimelinePropertyChangeDialog from './TimelinePropertyChangeDialog';
import { TimelinePageTemplateUIProps } from './TimelinePageTemplateUI';
import { TimelinePostSendCallback } from './TimelinePostEdit';
import { UiLogicError } from '../common';

export interface TimelinePageTemplateProps<
  TManageItem,
  TTimeline extends TimelineInfo
> {
  name: string;
  onManage: (item: TManageItem) => void;
  UiComponent: React.ComponentType<
    ExcludeKey<TimelinePageTemplateUIProps<TManageItem>, 'CardComponent'>
  >;
  dataVersion?: number;
  notFoundI18nKey: string;
}

export default function TimelinePageTemplate<
  TManageItem,
  TTimeline extends TimelineInfo
>(
  props: TimelinePageTemplateProps<TManageItem, TTimeline>
): React.ReactElement | null {
  const { t } = useTranslation();

  const { name } = props;

  const service = timelineService;

  const user = useUser();

  const [dialog, setDialog] = React.useState<null | 'property' | 'member'>(
    null
  );
  const [timeline, setTimeline] = React.useState<TimelineInfo | undefined>(
    undefined
  );
  const [posts, setPosts] = React.useState<
    TimelinePostInfoEx[] | 'forbid' | undefined
  >(undefined);
  const [error, setError] = React.useState<string | undefined>(undefined);

  React.useEffect(() => {
    const subscription = service
      .getTimeline(name)
      .pipe(
        switchMap((ti) => {
          setTimeline(ti);
          if (!service.hasReadPermission(user, ti)) {
            setPosts('forbid');
            return of(null);
          } else {
            return service
              .getPosts(name)
              .pipe(map((ps) => ({ timeline: ti, posts: ps })));
          }
        })
      )
      .subscribe(
        (data) => {
          if (data != null) {
            setPosts(
              data.posts.map((post) => ({
                ...post,
                deletable: service.hasModifyPostPermission(
                  user,
                  data.timeline,
                  post
                ),
              }))
            );
          }
        },
        (error) => {
          if (error instanceof TimelineNotExistError) {
            setError(t(props.notFoundI18nKey));
          } else {
            setError(
              // TODO: Convert this to a function.
              (error as { message?: string })?.message ?? 'Unknown error'
            );
          }
        }
      );
    return () => {
      subscription.unsubscribe();
    };
  }, [name, service, user, t, props.dataVersion, props.notFoundI18nKey]);

  const closeDialog = React.useCallback((): void => {
    setDialog(null);
  }, []);

  let dialogElement: React.ReactElement | undefined;

  if (dialog === 'property') {
    if (timeline == null) {
      throw new UiLogicError(
        'Timeline is null but attempt to open change property dialog.'
      );
    }

    dialogElement = (
      <TimelinePropertyChangeDialog
        open
        close={closeDialog}
        oldInfo={{
          visibility: timeline.visibility,
          description: timeline.description,
        }}
        onProcess={(req) => {
          return service
            .changeTimelineProperty(name, req)
            .pipe(
              map((newTimeline) => {
                setTimeline(newTimeline);
              })
            )
            .toPromise();
        }}
      />
    );
  } else if (dialog === 'member') {
    if (timeline == null) {
      throw new UiLogicError(
        'Timeline is null but attempt to open change property dialog.'
      );
    }

    dialogElement = (
      <TimelineMemberDialog
        open
        onClose={closeDialog}
        members={[timeline.owner, ...timeline.members]}
        edit={
          service.hasManagePermission(user, timeline)
            ? {
                onCheckUser: (u) => {
                  return userInfoService
                    .getUserInfo(u)
                    .pipe(
                      catchError((e) => {
                        if (e instanceof UserNotExistError) {
                          return of(null);
                        } else {
                          throw e;
                        }
                      })
                    )
                    .toPromise();
                },
                onAddUser: (u) => {
                  return service
                    .addMember(name, u.username)
                    .pipe(
                      map(() => {
                        setTimeline({
                          ...timeline,
                          members: concat(timeline.members, u),
                        });
                      })
                    )
                    .toPromise();
                },
                onRemoveUser: (u) => {
                  service.removeMember(name, u).subscribe(() => {
                    const toDelete = timeline.members.find(
                      (m) => m.username === u
                    );
                    if (toDelete == null) {
                      throw new UiLogicError(
                        'The member to delete is not in list.'
                      );
                    }
                    setTimeline({
                      ...timeline,
                      members: without(timeline.members, toDelete),
                    });
                  });
                },
              }
            : null
        }
      />
    );
  }

  const { UiComponent } = props;

  const onDelete: TimelineDeleteCallback = React.useCallback(
    (index, id) => {
      service.deletePost(name, id).subscribe(
        () => {
          setPosts((oldPosts) =>
            without(
              oldPosts as TimelinePostInfoEx[],
              (oldPosts as TimelinePostInfoEx[])[index]
            )
          );
        },
        () => {
          pushAlert({
            type: 'danger',
            message: t('timeline.deletePostFailed'),
          });
        }
      );
    },
    [service, name, t]
  );

  const onPost: TimelinePostSendCallback = React.useCallback(
    (req) => {
      return service
        .createPost(name, req)
        .pipe(
          map((newPost) => {
            setPosts((oldPosts) =>
              concat(oldPosts as TimelinePostInfoEx[], {
                ...newPost,
                deletable: true,
              })
            );
          })
        )
        .toPromise();
    },
    [service, name]
  );

  const onManageProp = props.onManage;

  const onManage = React.useCallback(
    (item: 'property' | TManageItem) => {
      if (item === 'property') {
        setDialog(item);
      } else {
        onManageProp(item);
      }
    },
    [onManageProp]
  );

  const onMember = React.useCallback(() => {
    setDialog('member');
  }, []);

  return (
    <>
      <UiComponent
        error={error}
        timeline={timeline}
        posts={posts}
        onDelete={onDelete}
        onPost={
          timeline != null && service.hasPostPermission(user, timeline)
            ? onPost
            : undefined
        }
        onManage={
          timeline != null && service.hasManagePermission(user, timeline)
            ? onManage
            : undefined
        }
        onMember={onMember}
      />
      {dialogElement}
    </>
  );
}
