import { useParams } from "react-router-dom";

import { UiLogicError } from "~src/common";

import Timeline from "./Timeline";

export default function TimelinePage() {
  const { owner, timeline: timelineNameParam } = useParams();

  if (owner == null || owner == "")
    throw new UiLogicError("Route param owner is not set.");

  const timeline = timelineNameParam || "self";

  return <Timeline timelineOwner={owner} timelineName={timeline} />;
};
