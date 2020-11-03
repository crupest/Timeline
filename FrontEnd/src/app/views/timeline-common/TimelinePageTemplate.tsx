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

import { TimelineDeleteCallback } from "./Timeline";
import { TimelineMemberDialog } from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";
import { TimelinePageTemplateUIProps } from "./TimelinePageTemplateUI";
import { TimelinePostSendCallback } from "./TimelinePostEdit";

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

  const timeline = timelineState?.timeline;

  const postListState = usePostList(name);

  const error: string | undefined = (() => {
    if (timelineState != null) {
      const { type, timeline } = timelineState;
      if (type === "offline" && timeline == null) return "Network Error";
      if (type === "synced" && timeline == null)
        return t(props.notFoundI18nKey);
    }
    return undefined;
  })();

  const closeDialog = React.useCallback((): void => {
    setDialog(null);
  }, []);

  let dialogElement: React.ReactElement | undefined;

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

  const onDelete: TimelineDeleteCallback = React.useCallback(
    (index, id) => {
      service.deletePost(name, id).subscribe(null, () => {
        pushAlert({
          type: "danger",
          message: t("timeline.deletePostFailed"),
        });
      });
    },
    [service, name, t]
  );

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

  return (
    <>
      <UiComponent
        error={error}
        timeline={timeline ?? undefined}
        postListState={postListState}
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
        onMember={() => setDialog("member")}
      />
      {dialogElement}
    </>
  );
}
