import React from "react";
import { useTranslation } from "react-i18next";
import { Container } from "react-bootstrap";

import { HttpNetworkError, HttpNotFoundError } from "@/http/common";
import { getHttpTimelineClient, HttpTimelineInfo } from "@/http/timeline";

import { getAlertHost } from "@/services/alert";

import Timeline from "./Timeline";
import TimelinePostEdit from "./TimelinePostEdit";

import useReverseScrollPositionRemember from "@/utilities/useReverseScrollPositionRemember";

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

  const [state, setState] = React.useState<
    "loading" | "done" | "offline" | "notexist" | "error"
  >("loading");
  const [timeline, setTimeline] = React.useState<HttpTimelineInfo | null>(null);

  useReverseScrollPositionRemember();

  React.useEffect(() => {
    setState("loading");
    setTimeline(null);
  }, [timelineName]);

  React.useEffect(() => {
    let subscribe = true;
    void getHttpTimelineClient()
      .getTimeline(timelineName)
      .then(
        (data) => {
          if (subscribe) {
            setState("done");
            setTimeline(data);
          }
        },
        (error) => {
          if (subscribe) {
            if (error instanceof HttpNetworkError) {
              setState("offline");
            } else if (error instanceof HttpNotFoundError) {
              setState("notexist");
            } else {
              console.error(error);
              setState("error");
            }
            setTimeline(null);
          }
        }
      );
    return () => {
      subscribe = false;
    };
  }, [timelineName, reloadKey]);

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

  return (
    <>
      {timeline != null ? (
        <CardComponent
          className="timeline-template-card"
          timeline={timeline}
          collapse={cardCollapse}
          toggleCollapse={toggleCardCollapse}
          onReload={onReload}
        />
      ) : null}
      <Container
        className="px-0"
        style={{
          minHeight: `calc(100vh - ${56 + bottomSpaceHeight}px)`,
        }}
      >
        {(() => {
          if (state === "offline") {
            // TODO: i18n
            return <p className="text-danger">Offline!</p>;
          } else if (state === "notexist") {
            return <p className="text-danger">{t(props.notFoundI18nKey)}</p>;
          } else if (state === "error") {
            // TODO: i18n
            return <p className="text-danger">Error!</p>;
          } else {
            return (
              <Timeline
                timelineName={timeline?.name}
                reloadKey={timelineReloadKey}
                onReload={reloadTimeline}
              />
            );
          }
        })()}
      </Container>
      {timeline != null && timeline.postable ? (
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
};

export default TimelinePageTemplate;
