import React from "react";
import { HubConnectionState } from "@microsoft/signalr";
import { useParams } from "react-router-dom";

import { UiLogicError } from "@/common";
import { HttpTimelineInfo } from "@/http/timeline";
import useReverseScrollPositionRemember from "@/utilities/useReverseScrollPositionRemember";
import { generatePalette, setPalette } from "@/palette";

import Timeline from "./Timeline";
import TimelineCard from "./TimelineCard";

const TimelinePage: React.FC = () => {
  const { owner: ownerUsername, timeline: timelineNameParam } = useParams();

  if (ownerUsername == null || ownerUsername == "")
    throw new UiLogicError("Route param owner is not set.");

  const timelineName =
    timelineNameParam == null || timelineNameParam === ""
      ? "self"
      : timelineNameParam;

  const [timeline, setTimeline] = React.useState<HttpTimelineInfo | null>(null);

  const [reloadKey, setReloadKey] = React.useState<number>(0);
  const reload = (): void => setReloadKey(reloadKey + 1);

  const [connectionStatus, setConnectionStatus] =
    React.useState<HubConnectionState>(HubConnectionState.Connecting);

  useReverseScrollPositionRemember();

  React.useEffect(() => {
    if (timeline != null && timeline.color != null) {
      return setPalette(generatePalette({ primary: timeline.color }));
    }
  }, [timeline]);

  const cardCollapseLocalStorageKey = `timeline.${ownerUsername}.${timelineName}.cardCollapse`;

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
        <TimelineCard
          className="timeline-template-card"
          timeline={timeline}
          collapse={cardCollapse}
          toggleCollapse={toggleCardCollapse}
          onReload={reload}
          connectionStatus={connectionStatus}
        />
      ) : null}
      <div className="container">
        <Timeline
          timelineOwner={ownerUsername}
          timelineName={timelineName}
          reloadKey={reloadKey}
          onReload={reload}
          onTimelineLoaded={(t) => setTimeline(t)}
          onConnectionStateChanged={setConnectionStatus}
        />
      </div>
    </>
  );
};

export default TimelinePage;
