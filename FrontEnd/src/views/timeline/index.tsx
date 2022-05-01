import React from "react";
import { useParams } from "react-router-dom";

import { UiLogicError } from "@/common";

import Timeline from "./Timeline";

const TimelinePage: React.FC = () => {
  const { owner, timeline: timelineNameParam } = useParams();

  if (owner == null || owner == "")
    throw new UiLogicError("Route param owner is not set.");

  const timeline = timelineNameParam || "self";

  return (
    <div className="container">
      <Timeline timelineOwner={owner} timelineName={timeline} />
    </div>
  );
};

export default TimelinePage;
