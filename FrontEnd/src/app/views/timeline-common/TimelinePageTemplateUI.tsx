import React from "react";
import { useTranslation } from "react-i18next";
import { Spinner } from "react-bootstrap";

import { getAlertHost } from "@/services/alert";
import { TimelineInfo, TimelinePostInfo } from "@/services/timeline";

import Timeline from "./Timeline";
import TimelinePostEdit, { TimelinePostSendCallback } from "./TimelinePostEdit";
import { TimelineSyncStatus } from "./SyncStatusBadge";

export interface TimelineCardComponentProps<TManageItems> {
  timeline: TimelineInfo;
  syncStatus: TimelineSyncStatus;
  operations: {
    onManage?: (item: TManageItems | "property") => void;
    onMember: () => void;
  };
  collapse: boolean;
  toggleCollapse: () => void;
  className?: string;
}

export interface TimelinePageTemplateUIOperations<TManageItems> {
  onManage?: (item: TManageItems | "property") => void;
  onMember: () => void;
  onBookmark?: () => void;
  onHighlight?: () => void;
  onPost?: TimelinePostSendCallback;
}

export interface TimelinePageTemplateUIProps<TManageItems> {
  timeline?:
    | (TimelineInfo & {
        operations: TimelinePageTemplateUIOperations<TManageItems>;
        posts?: TimelinePostInfo[] | "forbid";
      })
    | "notexist"
    | "offline";
  syncStatus: TimelineSyncStatus;
  notExistMessageI18nKey: string;
  CardComponent: React.ComponentType<TimelineCardComponentProps<TManageItems>>;
}

export default function TimelinePageTemplateUI<TManageItems>(
  props: TimelinePageTemplateUIProps<TManageItems>
): React.ReactElement | null {
  const { timeline, syncStatus, CardComponent } = props;

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

  const timelineName = typeof timeline === "object" ? timeline.name : null;

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

  if (timeline == null) {
    body = (
      <div className="full-viewport-center-child">
        <Spinner variant="primary" animation="grow" />
      </div>
    );
  } else if (timeline === "offline") {
    // TODO: i18n
    body = <p className="text-danger">Offline!</p>;
  } else if (timeline === "notexist") {
    body = <p className="text-danger">{t(props.notExistMessageI18nKey)}</p>;
  } else {
    const { operations, posts } = timeline;
    body = (
      <>
        <CardComponent
          className="timeline-template-card"
          timeline={timeline}
          operations={operations}
          syncStatus={syncStatus}
          collapse={cardCollapse}
          toggleCollapse={toggleCardCollapse}
        />
        {posts != null ? (
          posts === "forbid" ? (
            <div>{t("timeline.messageCantSee")}</div>
          ) : (
            <div
              className="timeline-container"
              style={{
                minHeight: `calc(100vh - ${56 + bottomSpaceHeight}px)`,
              }}
            >
              <Timeline timeline={timeline} posts={posts} />
            </div>
          )
        ) : (
          <div className="full-viewport-center-child">
            <Spinner variant="primary" animation="grow" />
          </div>
        )}
        {operations.onPost != null ? (
          <>
            <div
              style={{ height: bottomSpaceHeight }}
              className="flex-fix-length"
            />
            <TimelinePostEdit
              className="fixed-bottom"
              onPost={operations.onPost}
              onHeightChange={onPostEditHeightChange}
              timelineUniqueId={timeline.uniqueId}
            />
          </>
        ) : null}
      </>
    );
  }
  return body;
}
