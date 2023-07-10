import * as React from "react";

import TimelineEmptyItem from "./TimelineEmptyItem";

const TimelineLoading: React.FC = () => {
  return (
    <TimelineEmptyItem
      className="timeline-top-loading-enter"
      height={100}
      center="loading"
      startSegmentLength={56}
    />
  );
};

export default TimelineLoading;
