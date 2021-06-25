import React from "react";
import { Container } from "react-bootstrap";
import { HubConnectionState } from "@microsoft/signalr";

import { HttpTimelineInfo } from "@/http/timeline";

import useReverseScrollPositionRemember from "@/utilities/useReverseScrollPositionRemember";

import { generatePalette, setPalette } from "@/palette";

import Timeline from "./Timeline";

export interface TimelinePageCardProps {
  timeline: HttpTimelineInfo;
  collapse: boolean;
  toggleCollapse: () => void;
  connectionStatus: HubConnectionState;
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

  const [timeline, setTimeline] = React.useState<HttpTimelineInfo | null>(null);

  const [connectionStatus, setConnectionStatus] =
    React.useState<HubConnectionState>(HubConnectionState.Connecting);

  useReverseScrollPositionRemember();

  React.useEffect(() => {
    if (timeline != null && timeline.color != null) {
      return setPalette(generatePalette({ primary: timeline.color }));
    }
  }, [timeline]);

  const cardCollapseLocalStorageKey = `timeline.${timelineName}.cardCollapse`;

  const [cardCollapse, setCardCollapse] = React.useState<boolean>(true);

  React.useEffect(() => {
    const savedCollapse = window.localStorage.getItem(
      cardCollapseLocalStorageKey
    );
    setCardCollapse(savedCollapse == null ? true : savedCollapse === "true");
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
          connectionStatus={connectionStatus}
        />
      ) : null}
      <div className="container">
        <Timeline
          timelineName={timelineName}
          reloadKey={reloadKey}
          onReload={onReload}
          onTimelineLoaded={(t) => setTimeline(t)}
          onConnectionStateChanged={setConnectionStatus}
        />
      </div>
    </>
  );
};

export default TimelinePageTemplate;
