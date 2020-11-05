import React from "react";
import { useTranslation } from "react-i18next";
import { fromEvent } from "rxjs";
import { Spinner } from "react-bootstrap";

import { getAlertHost } from "@/services/alert";
import { useEventEmiiter, UiLogicError } from "@/common";
import {
  TimelineInfo,
  TimelinePostsWithSyncState,
  timelineService,
} from "@/services/timeline";
import { userService } from "@/services/user";

import Timeline, {
  TimelinePostInfoEx,
  TimelineDeleteCallback,
} from "./Timeline";
import TimelinePostEdit, { TimelinePostSendCallback } from "./TimelinePostEdit";
import { TimelineSyncStatus } from "./SyncStatusBadge";

export interface TimelineCardComponentProps<TManageItems> {
  timeline: TimelineInfo;
  onManage?: (item: TManageItems | "property") => void;
  onMember: () => void;
  className?: string;
  collapse: boolean;
  syncStatus: TimelineSyncStatus;
  toggleCollapse: () => void;
}

export interface TimelinePageTemplateUIProps<TManageItems> {
  timeline?: TimelineInfo;
  postListState?: TimelinePostsWithSyncState;
  CardComponent: React.ComponentType<TimelineCardComponentProps<TManageItems>>;
  onMember: () => void;
  onManage?: (item: TManageItems | "property") => void;
  onPost?: TimelinePostSendCallback;
  onDelete: TimelineDeleteCallback;
  error?: string;
}

export default function TimelinePageTemplateUI<TManageItems>(
  props: TimelinePageTemplateUIProps<TManageItems>
): React.ReactElement | null {
  const { timeline, postListState } = props;

  const { t } = useTranslation();

  const [bottomSpaceHeight, setBottomSpaceHeight] = React.useState<number>(0);

  const onPostEditHeightChange = React.useCallback((height: number): void => {
    setBottomSpaceHeight(height);
    if (height === 0) {
      const alertHost = getAlertHost();
      if (alertHost != null) {
        alertHost.style.removeProperty("margin-bottom");
      }
    } else {
      const alertHost = getAlertHost();
      if (alertHost != null) {
        alertHost.style.marginBottom = `${height}px`;
      }
    }
  }, []);

  const timelineRef = React.useRef<HTMLDivElement | null>(null);

  const [getResizeEvent, triggerResizeEvent] = useEventEmiiter();

  React.useEffect(() => {
    const { current: timelineElement } = timelineRef;
    if (timelineElement != null) {
      let loadingScrollToBottom = true;
      let pinBottom = false;

      const isAtBottom = (): boolean =>
        window.innerHeight + window.scrollY + 10 >= document.body.scrollHeight;

      const disableLoadingScrollToBottom = (): void => {
        loadingScrollToBottom = false;
        if (isAtBottom()) pinBottom = true;
      };

      const checkAndScrollToBottom = (): void => {
        if (loadingScrollToBottom || pinBottom) {
          window.scrollTo(0, document.body.scrollHeight);
        }
      };

      const subscriptions = [
        fromEvent(timelineElement, "wheel").subscribe(
          disableLoadingScrollToBottom
        ),
        fromEvent(timelineElement, "pointerdown").subscribe(
          disableLoadingScrollToBottom
        ),
        fromEvent(timelineElement, "keydown").subscribe(
          disableLoadingScrollToBottom
        ),
        fromEvent(window, "scroll").subscribe(() => {
          if (loadingScrollToBottom) return;

          if (isAtBottom()) {
            pinBottom = true;
          } else {
            pinBottom = false;
          }
        }),
        fromEvent(window, "resize").subscribe(checkAndScrollToBottom),
        getResizeEvent().subscribe(checkAndScrollToBottom),
      ];

      return () => {
        subscriptions.forEach((s) => s.unsubscribe());
      };
    }
  }, [getResizeEvent, triggerResizeEvent, timeline, postListState]);

  const genCardCollapseLocalStorageKey = (uniqueId: string): string =>
    `timeline.${uniqueId}.cardCollapse`;

  const cardCollapseLocalStorageKey =
    timeline != null ? genCardCollapseLocalStorageKey(timeline.uniqueId) : null;

  const [cardCollapse, setCardCollapse] = React.useState<boolean>(true);
  React.useEffect(() => {
    if (cardCollapseLocalStorageKey != null) {
      const savedCollapse =
        window.localStorage.getItem(cardCollapseLocalStorageKey) === "true";
      setCardCollapse(savedCollapse);
    }
  }, [cardCollapseLocalStorageKey]);

  const toggleCardCollapse = (): void => {
    const newState = !cardCollapse;
    setCardCollapse(newState);
    if (timeline != null) {
      window.localStorage.setItem(
        genCardCollapseLocalStorageKey(timeline.uniqueId),
        newState.toString()
      );
    }
  };

  let body: React.ReactElement;

  if (props.error != null) {
    body = <p className="text-danger">{t(props.error)}</p>;
  } else {
    if (timeline != null) {
      let timelineBody: React.ReactElement;
      if (postListState != null) {
        if (postListState.type === "notexist") {
          throw new UiLogicError(
            "Timeline is not null but post list state is notexist."
          );
        }
        if (postListState.type === "forbid") {
          timelineBody = (
            <p className="text-danger">{t("timeline.messageCantSee")}</p>
          );
        } else {
          const posts: TimelinePostInfoEx[] = postListState.posts.map(
            (post) => ({
              ...post,
              deletable: timelineService.hasModifyPostPermission(
                userService.currentUser,
                timeline,
                post
              ),
            })
          );

          timelineBody = (
            <Timeline
              containerRef={timelineRef}
              posts={posts}
              onDelete={props.onDelete}
              onResize={triggerResizeEvent}
            />
          );
          if (props.onPost != null) {
            timelineBody = (
              <>
                {timelineBody}
                <div
                  style={{ height: bottomSpaceHeight }}
                  className="flex-fix-length"
                />
                <TimelinePostEdit
                  className="fixed-bottom"
                  onPost={props.onPost}
                  onHeightChange={onPostEditHeightChange}
                  timelineUniqueId={timeline.uniqueId}
                />
              </>
            );
          }
        }
      } else {
        timelineBody = (
          <div className="full-viewport-center-child">
            <Spinner variant="primary" animation="grow" />
          </div>
        );
      }

      const { CardComponent } = props;
      const syncStatus: TimelineSyncStatus =
        postListState == null || postListState.syncing
          ? "syncing"
          : postListState.type === "synced"
          ? "synced"
          : "offline";

      body = (
        <>
          <div className="timeline-background" />
          <CardComponent
            className="timeline-template-card"
            timeline={timeline}
            onManage={props.onManage}
            onMember={props.onMember}
            syncStatus={syncStatus}
            collapse={cardCollapse}
            toggleCollapse={toggleCardCollapse}
          />
          {timelineBody}
        </>
      );
    } else {
      body = (
        <div className="full-viewport-center-child">
          <Spinner variant="primary" animation="grow" />
        </div>
      );
    }
  }

  return body;
}
