import React from "react";
import { useParams } from "react-router-dom";

import { UiLogicError } from "@/common";

import TimelinePageTemplate from "../timeline-common/TimelinePageTemplate";
import TimelineCard from "./TimelineCard";

const TimelinePage: React.FC = () => {
  const { name } = useParams();

  if (name == null) {
    throw new UiLogicError("No route param 'name'.");
  }

  const [reloadKey, setReloadKey] = React.useState<number>(0);

  return (
    <TimelinePageTemplate
      timelineName={name}
      notFoundI18nKey="timeline.timelineNotExist"
      reloadKey={reloadKey}
      CardComponent={TimelineCard}
      onReload={() => setReloadKey(reloadKey + 1)}
    />
  );
};

export default TimelinePage;
