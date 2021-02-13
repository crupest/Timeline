import React from "react";
import { useTranslation } from "react-i18next";
import { Spinner } from "react-bootstrap";

import { getAlertHost } from "@/services/alert";

import { HttpTimelineInfo, HttpTimelinePostInfo } from "@/http/timeline";

import Timeline from "./Timeline";
import TimelinePostEdit from "./TimelinePostEdit";

export interface TimelineCardComponentProps<TManageItems> {
  timeline: HttpTimelineInfo;
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
}

export interface TimelinePageTemplateUIProps<TManageItems> {
  timeline:
    | (HttpTimelineInfo & {
        operations: TimelinePageTemplateUIOperations<TManageItems>;
      })
    | "notexist"
    | "offline"
    | "loading"
    | "error";
  notExistMessageI18nKey: string;
  CardComponent: React.ComponentType<TimelineCardComponentProps<TManageItems>>;
}

export default function TimelinePageTemplateUI<TManageItems>(
  props: TimelinePageTemplateUIProps<TManageItems>
): React.ReactElement | null {
  const { timeline, CardComponent } = props;

  const { t } = useTranslation();

  const scrollToBottom = React.useCallback(() => {
    window.scrollTo(0, document.body.scrollHeight);
  }, []);

  const [bottomSpaceHeight, setBottomSpaceHeight] = React.useState<number>(0);

  const [timelineReloadKey, setTimelineReloadKey] = React.useState<number>(0);

  const [newPosts, setNewPosts] = React.useState<HttpTimelinePostInfo[]>([]);

  const reloadTimeline = (): void => {
    setTimelineReloadKey((old) => old + 1);
    setNewPosts([]);
  };

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

  if (timeline == "loading") {
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
  } else if (timeline === "error") {
    // TODO: i18n
    body = <p className="text-danger">Error!</p>;
  } else {
    const { operations } = timeline;
    body = (
      <>
        <CardComponent
          className="timeline-template-card"
          timeline={timeline}
          operations={operations}
          collapse={cardCollapse}
          toggleCollapse={toggleCardCollapse}
        />
        <div
          className="timeline-container"
          style={{
            minHeight: `calc(100vh - ${56 + bottomSpaceHeight}px)`,
          }}
        >
          <Timeline
            timelineName={timeline.name}
            reloadKey={timelineReloadKey}
            onReload={reloadTimeline}
            additionalPosts={newPosts}
            onLoad={scrollToBottom}
          />
        </div>
        {timeline.postable ? (
          <>
            <div
              style={{ height: bottomSpaceHeight }}
              className="flex-fix-length"
            />
            <TimelinePostEdit
              className="fixed-bottom"
              timeline={timeline}
              onHeightChange={onPostEditHeightChange}
              onPosted={(newPost) => {
                setNewPosts((old) => [...old, newPost]);
              }}
            />
          </>
        ) : null}
      </>
    );
  }
  return body;
}
