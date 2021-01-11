import React from "react";
import { useTranslation } from "react-i18next";

import { UiLogicError } from "@/common";
import { pushAlert } from "@/services/alert";
import { useUser } from "@/services/user";
import { timelineService, usePosts, useTimeline } from "@/services/timeline";
import { getHttpBookmarkClient } from "@/http/bookmark";
import { getHttpHighlightClient } from "@/http/highlight";

import { TimelineMemberDialog } from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";
import { TimelinePageTemplateUIProps } from "./TimelinePageTemplateUI";
import { TimelinePostSendCallback } from "./TimelinePostEdit";
import { TimelinePostInfoEx } from "./Timeline";
import { mergeDataStatus } from "@/services/DataHub2";

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

  const timelineAndStatus = useTimeline(name);
  const postsAndState = usePosts(name);

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

  const data = ((): TimelinePageTemplateUIProps<TManageItem>["data"] => {
    const { status, data: timeline } = timelineAndStatus;
    if (timeline == null) {
      if (status === "offline") {
        return { type: "custom", value: "Network Error" };
      } else {
        return undefined;
      }
    } else if (timeline === "notexist") {
      return props.notFoundI18nKey;
    } else {
      const posts = ((): TimelinePostInfoEx[] | "forbid" | undefined => {
        const { data: postsInfo } = postsAndState;
        if (postsInfo === "forbid") {
          return "forbid";
        } else if (postsInfo == null || postsInfo === "notexist") {
          return undefined;
        } else {
          return postsInfo.posts.map((post) => ({
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
        }
      })();

      const operations = {
        onPost: service.hasPostPermission(user, timeline) ? onPost : undefined,
        onManage: service.hasManagePermission(user, timeline)
          ? onManage
          : undefined,
        onMember: () => setDialog("member"),
        onBookmark:
          user != null
            ? () => {
                const { isBookmark } = timeline;
                const client = getHttpBookmarkClient();
                const promise = isBookmark
                  ? client.delete(name)
                  : client.put(name);
                promise.then(
                  () => {
                    void timelineService.syncTimeline(name);
                  },
                  () => {
                    pushAlert({
                      message: {
                        type: "i18n",
                        key: isBookmark
                          ? "timeline.removeBookmarkFail"
                          : "timeline.addBookmarkFail",
                      },
                      type: "danger",
                    });
                  }
                );
              }
            : undefined,
        onHighlight:
          user != null && user.hasHighlightTimelineAdministrationPermission
            ? () => {
                const { isHighlight } = timeline;
                const client = getHttpHighlightClient();
                const promise = isHighlight
                  ? client.delete(name)
                  : client.put(name);
                promise.then(
                  () => {
                    void timelineService.syncTimeline(name);
                  },
                  () => {
                    pushAlert({
                      message: {
                        type: "i18n",
                        key: isHighlight
                          ? "timeline.removeHighlightFail"
                          : "timeline.addHighlightFail",
                      },
                      type: "danger",
                    });
                  }
                );
              }
            : undefined,
      };

      return { timeline, posts, operations };
    }
  })();

  const closeDialog = React.useCallback((): void => {
    setDialog(null);
  }, []);

  let dialogElement: React.ReactElement | undefined;

  const timeline = timelineAndStatus?.data;

  if (dialog === "property") {
    if (timeline == null || timeline === "notexist") {
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
    if (timeline == null || timeline === "notexist") {
      throw new UiLogicError(
        "Timeline is null but attempt to open change property dialog."
      );
    }

    dialogElement = (
      <TimelineMemberDialog
        open
        onClose={closeDialog}
        timeline={timeline}
        editable={service.hasManagePermission(user, timeline)}
      />
    );
  }

  const { UiComponent } = props;

  return (
    <>
      <UiComponent
        data={data}
        syncStatus={mergeDataStatus([
          timelineAndStatus.status,
          postsAndState.status,
        ])}
      />
      {dialogElement}
    </>
  );
}
