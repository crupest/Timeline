import React from "react";
import { useTranslation } from "react-i18next";

import { UiLogicError } from "@/common";
import { pushAlert } from "@/services/alert";
import { useUser } from "@/services/user";
import {
  TimelinePostInfo,
  timelineService,
  usePosts,
  useTimeline,
} from "@/services/timeline";
import { mergeDataStatus } from "@/services/DataHub2";

import { TimelineMemberDialog } from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";
import {
  TimelinePageTemplateUIOperations,
  TimelinePageTemplateUIProps,
} from "./TimelinePageTemplateUI";

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

  const [scrollBottomKey, setScrollBottomKey] = React.useState<number>(0);

  React.useEffect(() => {
    if (scrollBottomKey > 0) {
      window.scrollTo(0, document.body.scrollHeight);
    }
  }, [scrollBottomKey]);

  const timelineAndStatus = useTimeline(name);
  const postsAndState = usePosts(name);

  const [
    scrollToBottomNextSyncKey,
    setScrollToBottomNextSyncKey,
  ] = React.useState<number>(0);

  const scrollToBottomNextSync = (): void => {
    setScrollToBottomNextSyncKey((old) => old + 1);
  };

  React.useEffect(() => {
    let subscribe = true;
    void timelineService.syncPosts(name).then(() => {
      if (subscribe) {
        setScrollBottomKey((old) => old + 1);
      }
    });
    return () => {
      subscribe = false;
    };
  }, [name, scrollToBottomNextSyncKey]);

  const uiTimelineProp = ((): TimelinePageTemplateUIProps<TManageItem>["timeline"] => {
    const { status, data: timeline } = timelineAndStatus;
    if (timeline == null) {
      if (status === "offline") {
        return "offline";
      } else {
        return undefined;
      }
    } else if (timeline === "notexist") {
      return "notexist";
    } else {
      const operations: TimelinePageTemplateUIOperations<TManageItem> = {
        onDeletePost: (post) => {
          service.deletePost(name, post.id).catch(() => {
            pushAlert({
              type: "danger",
              message: t("timeline.deletePostFailed"),
            });
          });
        },
        onPost: service.hasPostPermission(user, timeline)
          ? (req) =>
              service.createPost(name, req).then(() => scrollToBottomNextSync())
          : undefined,
        onManage: service.hasManagePermission(user, timeline)
          ? (item) => {
              if (item === "property") {
                setDialog(item);
              } else {
                props.onManage(item);
              }
            }
          : undefined,
        onMember: () => setDialog("member"),
        onBookmark:
          user != null
            ? () => {
                service
                  .setBookmark(timeline.name, !timeline.isBookmark)
                  .catch(() => {
                    pushAlert({
                      message: {
                        type: "i18n",
                        key: timeline.isBookmark
                          ? "timeline.removeBookmarkFail"
                          : "timeline.addBookmarkFail",
                      },
                      type: "danger",
                    });
                  });
              }
            : undefined,
        onHighlight:
          user != null && user.hasHighlightTimelineAdministrationPermission
            ? () => {
                service
                  .setHighlight(timeline.name, !timeline.isHighlight)
                  .catch(() => {
                    pushAlert({
                      message: {
                        type: "i18n",
                        key: timeline.isHighlight
                          ? "timeline.removeHighlightFail"
                          : "timeline.addHighlightFail",
                      },
                      type: "danger",
                    });
                  });
              }
            : undefined,
      };

      const posts = ((): TimelinePostInfo[] | "forbid" | undefined => {
        const { data: postsInfo } = postsAndState;
        if (postsInfo === "forbid") {
          return "forbid";
        } else if (postsInfo == null || postsInfo === "notexist") {
          return undefined;
        } else {
          return postsInfo.posts;
        }
      })();

      return { ...timeline, operations, posts };
    }
  })();

  const timeline = timelineAndStatus?.data;
  let dialogElement: React.ReactElement | undefined;
  const closeDialog = (): void => setDialog(null);

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
        timeline={timeline}
        onProcess={(req) => service.changeTimelineProperty(name, req)}
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
        timeline={uiTimelineProp}
        syncStatus={mergeDataStatus([
          timelineAndStatus.status,
          postsAndState.status,
        ])}
        notExistMessageI18nKey={props.notFoundI18nKey}
      />
      {dialogElement}
    </>
  );
}
