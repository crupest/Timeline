import React from "react";
import { useParams } from "react-router";

import TimelinePageTemplate from "../timeline-common/TimelinePageTemplate";
import TimelineCard from "./TimelineCard";

const TimelinePage: React.FC = () => {
  const { name } = useParams<{ name: string }>();

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
