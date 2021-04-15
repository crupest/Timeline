import React from "react";

import TimelineTop from "./TimelineTop";

const TimelineLoading: React.FC = () => {
  return (
    <TimelineTop
      className="timeline-top-loading-enter"
      height={100}
      lineProps={{
        center: "loading",
        startSegmentLength: 56,
      }}
    />
  );
};

export default TimelineLoading;
