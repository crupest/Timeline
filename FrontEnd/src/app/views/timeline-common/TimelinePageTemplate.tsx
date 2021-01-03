import React from "react";
import { useTranslation } from "react-i18next";
import { of } from "rxjs";
import { catchError } from "rxjs/operators";

import { UiLogicError } from "@/common";
import { pushAlert } from "@/services/alert";
import { useUser, userInfoService, UserNotExistError } from "@/services/user";
import {
  timelineService,
  usePostList,
  useTimelineInfo,
} from "@/services/timeline";
import { getHttpBookmarkClient } from "@/http/bookmark";
import { getHttpHighlightClient } from "@/http/highlight";

import { TimelineMemberDialog } from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";
import { TimelinePageTemplateUIProps } from "./TimelinePageTemplateUI";
import { TimelinePostSendCallback } from "./TimelinePostEdit";
import { TimelineSyncStatus } from "./SyncStatusBadge";
import { TimelinePostInfoEx } from "./Timeline";

export interface TimelinePageTemplateProps<TManageItem> {
  name: string;
  onManage: (item: TManageItem) => void;
  UiComponent: React.ComponentType<
    Omit<TimelinePageTemplateUIProps<TManageItem>, "CardComponent">
  >;
  notFoundI18nKey: string;
}

export default function TimelinePageTemplate<TManageItem>(
  props: TimelinePageTemplateProps<TManageItem>
): React.ReactElement | null {
  const { t } = useTranslation();

  const { name } = props;

  const service = timelineService;

  const user = useUser();

  const [dialog, setDialog] = React.useState<null | "property" | "member">(
    null
  );

  const timelineState = useTimelineInfo(name);
  const postListState = usePostList(name);

  const onPost: TimelinePostSendCallback = React.useCallback(
    (req) => {
      return service.createPost(name, req).toPromise().then();
    },
    [service, name]
  );

  const onManageProp = props.onManage;

  const onManage = React.useCallback(
    (item: "property" | TManageItem) => {
      if (item === "property") {
        setDialog(item);
      } else {
        onManageProp(item);
      }
    },
    [onManageProp]
  );

  const childProps = ((): [
    data: TimelinePageTemplateUIProps<TManageItem>["data"],
    syncStatus: TimelineSyncStatus
  ] => {
    if (timelineState == null) {
      return [undefined, "syncing"];
    } else {
      const { type, timeline } = timelineState;
      if (timeline == null) {
        if (type === "offline") {
          return [{ type: "custom", value: "Network Error" }, "offline"];
        } else if (type === "synced") {
          return [props.notFoundI18nKey, "synced"];
        } else {
          return [undefined, "syncing"];
        }
      } else {
        if (postListState != null && postListState.type === "notexist") {
          return [props.notFoundI18nKey, "synced"];
        }
        if (postListState != null && postListState.type === "forbid") {
          return ["timeline.messageCantSee", "synced"];
        }

        const posts:
          | TimelinePostInfoEx[]
          | undefined = postListState?.posts?.map((post) => ({
          ...post,
          onDelete: service.hasModifyPostPermission(user, timeline, post)
            ? () => {
                service.deletePost(name, post.id).subscribe({
                  error: () => {
                    pushAlert({
                      type: "danger",
                      message: t("timeline.deletePostFailed"),
                    });
                  },
                });
              }
            : undefined,
        }));

        const others = {
          onPost: service.hasPostPermission(user, timeline)
            ? onPost
            : undefined,
          onManage: service.hasManagePermission(user, timeline)
            ? onManage
            : undefined,
          onMember: () => setDialog("member"),
          onBookmark:
            user != null
              ? () => {
                  void getHttpBookmarkClient()
                    .put(name, user.token)
                    .then(() => {
                      pushAlert({
                        message: {
                          type: "i18n",
                          key: "timeline.addBookmarkSuccess",
                        },
                        type: "success",
                      });
                    });
                }
              : undefined,
          onHighlight:
            user != null && user.hasHighlightTimelineAdministrationPermission
              ? () => {
                  void getHttpHighlightClient()
                    .put(name, user.token)
                    .then(() => {
                      pushAlert({
                        message: {
                          type: "i18n",
                          key: "timeline.addHighlightSuccess",
                        },
                        type: "success",
                      });
                    });
                }
              : undefined,
        };

        if (type === "cache") {
          return [{ timeline, posts, ...others }, "syncing"];
        } else if (type === "offline") {
          return [{ timeline, posts, ...others }, "offline"];
        } else {
          if (postListState == null) {
            return [{ timeline, posts, ...others }, "syncing"];
          } else {
            const { type: postListType } = postListState;
            if (postListType === "synced") {
              return [{ timeline, posts, ...others }, "synced"];
            } else if (postListType === "cache") {
              return [{ timeline, posts, ...others }, "syncing"];
            } else if (postListType === "offline") {
              return [{ timeline, posts, ...others }, "offline"];
            }
          }
        }
      }
    }
    throw new UiLogicError("Failed to calculate TimelinePageUITemplate props.");
  })();

  const closeDialog = React.useCallback((): void => {
    setDialog(null);
  }, []);

  let dialogElement: React.ReactElement | undefined;

  const timeline = timelineState?.timeline;

  if (dialog === "property") {
    if (timeline == null) {
      throw new UiLogicError(
        "Timeline is null but attempt to open change property dialog."
      );
    }

    dialogElement = (
      <TimelinePropertyChangeDialog
        open
        close={closeDialog}
        oldInfo={{
          title: timeline.title,
          visibility: timeline.visibility,
          description: timeline.description,
        }}
        onProcess={(req) => {
          return service.changeTimelineProperty(name, req).toPromise().then();
        }}
      />
    );
  } else if (dialog === "member") {
    if (timeline == null) {
      throw new UiLogicError(
        "Timeline is null but attempt to open change property dialog."
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
                  return service.addMember(name, u.username).toPromise().then();
                },
                onRemoveUser: (u) => {
                  service.removeMember(name, u);
                },
              }
            : null
        }
      />
    );
  }

  const { UiComponent } = props;

  return (
    <>
      <UiComponent data={childProps[0]} syncStatus={childProps[1]} />
      {dialogElement}
    </>
  );
}
