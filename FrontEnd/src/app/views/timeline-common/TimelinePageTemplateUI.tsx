import React from "react";
import { useTranslation } from "react-i18next";
import { fromEvent } from "rxjs";
import { Spinner } from "react-bootstrap";

import { getAlertHost } from "@/services/alert";
import { useEventEmiiter, I18nText, convertI18nText } from "@/common";
import { TimelineInfo } from "@/services/timeline";

import Timeline, { TimelinePostInfoEx } from "./Timeline";
import TimelinePostEdit, { TimelinePostSendCallback } from "./TimelinePostEdit";
import { TimelineSyncStatus } from "./SyncStatusBadge";

export interface TimelineCardComponentProps<TManageItems> {
  timeline: TimelineInfo;
  syncStatus: TimelineSyncStatus;
  operations: {
    onManage?: (item: TManageItems | "property") => void;
    onMember: () => void;
    onBookmark?: () => void;
    onHighlight?: () => void;
  };
  collapse: boolean;
  toggleCollapse: () => void;
  className?: string;
}

export interface TimelinePageTemplateUIProps<TManageItems> {
  data?:
    | {
        timeline: TimelineInfo;
        posts?: TimelinePostInfoEx[] | "forbid";
        operations: {
          onManage?: (item: TManageItems | "property") => void;
          onMember: () => void;
          onBookmark?: () => void;
          onHighlight?: () => void;
          onPost?: TimelinePostSendCallback;
        };
      }
    | I18nText;
  syncStatus: TimelineSyncStatus;
  CardComponent: React.ComponentType<TimelineCardComponentProps<TManageItems>>;
}

export default function TimelinePageTemplateUI<TManageItems>(
  props: TimelinePageTemplateUIProps<TManageItems>
): React.ReactElement | null {
  const { data, syncStatus, CardComponent } = props;

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

  const timelineName: string | null =
    typeof data === "object" && "timeline" in data ? data.timeline.name : null;

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
  }, [getResizeEvent, triggerResizeEvent, timelineName]);

  const cardCollapseLocalStorageKey =
    timelineName != null ? `timeline.${timelineName}.cardCollapse` : null;

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
    if (cardCollapseLocalStorageKey != null) {
      window.localStorage.setItem(
        cardCollapseLocalStorageKey,
        newState.toString()
      );
    }
  };

  let body: React.ReactElement;

  if (data != null && (typeof data === "string" || "type" in data)) {
    body = <p className="text-danger">{convertI18nText(data, t)}</p>;
  } else {
    const posts = data?.posts;

    body = (
      <>
        {data != null ? (
          <CardComponent
            className="timeline-template-card"
            timeline={data.timeline}
            operations={data.operations}
            syncStatus={syncStatus}
            collapse={cardCollapse}
            toggleCollapse={toggleCardCollapse}
          />
        ) : null}
        {posts != null ? (
          posts === "forbid" ? (
            <div>{t("timeline.messageCantSee")}</div>
          ) : (
            <div
              className="timeline-container"
              style={{ minHeight: `calc(100vh - ${56 + bottomSpaceHeight}px)` }}
            >
              <Timeline
                containerRef={timelineRef}
                posts={posts}
                onResize={triggerResizeEvent}
              />
            </div>
          )
        ) : (
          <div className="full-viewport-center-child">
            <Spinner variant="primary" animation="grow" />
          </div>
        )}
        {data != null && data.operations.onPost != null ? (
          <>
            <div
              style={{ height: bottomSpaceHeight }}
              className="flex-fix-length"
            />
            <TimelinePostEdit
              className="fixed-bottom"
              onPost={data.operations.onPost}
              onHeightChange={onPostEditHeightChange}
              timelineUniqueId={data.timeline.uniqueId}
            />
          </>
        ) : null}
      </>
    );
  }
  return body;
}
