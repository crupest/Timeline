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
  avatarKey?: string | number;
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

  const bottomSpaceRef = React.useRef<HTMLDivElement | null>(null);

  const onPostEditHeightChange = React.useCallback((height: number): void => {
    const { current: bottomSpaceDiv } = bottomSpaceRef;
    if (bottomSpaceDiv != null) {
      bottomSpaceDiv.style.height = `${height}px`;
    }
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

  const [infoCardCollapse, setInfoCardCollapse] = React.useState<boolean>(true);
  React.useEffect(() => {
    if (cardCollapseLocalStorageKey != null) {
      const savedCollapse =
        window.localStorage.getItem(cardCollapseLocalStorageKey) === "true";
      setInfoCardCollapse(savedCollapse);
    }
  }, [cardCollapseLocalStorageKey]);

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
                <div ref={bottomSpaceRef} className="flex-fix-length" />
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
          <CardComponent
            timeline={timeline}
            onManage={props.onManage}
            onMember={props.onMember}
            className="timeline-info-card"
            syncStatus={syncStatus}
            collapse={infoCardCollapse}
            toggleCollapse={() => {
              const newState = !infoCardCollapse;
              setInfoCardCollapse(newState);
              if (timeline != null) {
                window.localStorage.setItem(
                  genCardCollapseLocalStorageKey(timeline.uniqueId),
                  newState.toString()
                );
              }
            }}
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
