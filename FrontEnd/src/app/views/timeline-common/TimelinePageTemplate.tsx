import React from "react";
import { useTranslation } from "react-i18next";
import { Container, Spinner } from "react-bootstrap";

import { HttpNetworkError, HttpNotFoundError } from "@/http/common";
import { getHttpTimelineClient, HttpTimelineInfo } from "@/http/timeline";

import { getAlertHost } from "@/services/alert";

import Timeline from "./Timeline";
import TimelinePostEdit from "./TimelinePostEdit";

export interface TimelinePageCardProps {
  timeline: HttpTimelineInfo;
  collapse: boolean;
  toggleCollapse: () => void;
  className?: string;
  onReload: () => void;
}

export interface TimelinePageTemplateProps {
  timelineName: string;
  notFoundI18nKey: string;
  reloadKey: number;
  onReload: () => void;
  CardComponent: React.ComponentType<TimelinePageCardProps>;
}

const TimelinePageTemplate: React.FC<TimelinePageTemplateProps> = (props) => {
  const { timelineName, reloadKey, onReload, CardComponent } = props;

  const { t } = useTranslation();

  const [timeline, setTimeline] = React.useState<
    HttpTimelineInfo | "loading" | "offline" | "notexist" | "error"
  >("loading");

  React.useEffect(() => {
    setTimeline("loading");
  }, [timelineName]);

  React.useEffect(() => {
    let subscribe = true;
    void getHttpTimelineClient()
      .getTimeline(timelineName)
      .then(
        (data) => {
          if (subscribe) {
            setTimeline(data);
          }
        },
        (error) => {
          if (subscribe) {
            if (error instanceof HttpNetworkError) {
              setTimeline("offline");
            } else if (error instanceof HttpNotFoundError) {
              setTimeline("notexist");
            } else {
              console.error(error);
              setTimeline("error");
            }
          }
        }
      );
    return () => {
      subscribe = false;
    };
  }, [timelineName, reloadKey]);

  const scrollToBottom = React.useCallback(() => {
    window.scrollTo(0, document.body.scrollHeight);
  }, []);

  const [bottomSpaceHeight, setBottomSpaceHeight] = React.useState<number>(0);

  const [timelineReloadKey, setTimelineReloadKey] = React.useState<number>(0);

  const reloadTimeline = (): void => {
    setTimelineReloadKey((old) => old + 1);
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

  const cardCollapseLocalStorageKey = `timeline.${timelineName}.cardCollapse`;

  const [cardCollapse, setCardCollapse] = React.useState<boolean>(true);
  React.useEffect(() => {
    const savedCollapse =
      window.localStorage.getItem(cardCollapseLocalStorageKey) === "true";
    setCardCollapse(savedCollapse);
  }, [cardCollapseLocalStorageKey]);

  const toggleCardCollapse = (): void => {
    const newState = !cardCollapse;
    setCardCollapse(newState);
    window.localStorage.setItem(
      cardCollapseLocalStorageKey,
      newState.toString()
    );
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
    body = <p className="text-danger">{t(props.notFoundI18nKey)}</p>;
  } else if (timeline === "error") {
    // TODO: i18n
    body = <p className="text-danger">Error!</p>;
  } else {
    body = (
      <>
        <CardComponent
          className="timeline-template-card"
          timeline={timeline}
          collapse={cardCollapse}
          toggleCollapse={toggleCardCollapse}
          onReload={onReload}
        />
        <Container
          className="px-0"
          style={{
            minHeight: `calc(100vh - ${56 + bottomSpaceHeight}px)`,
          }}
        >
          <Timeline
            top={40}
            timelineName={timeline.name}
            reloadKey={timelineReloadKey}
            onReload={reloadTimeline}
            onLoad={scrollToBottom}
          />
        </Container>
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
              onPosted={reloadTimeline}
            />
          </>
        ) : null}
      </>
    );
  }
  return body;
};

export default TimelinePageTemplate;
