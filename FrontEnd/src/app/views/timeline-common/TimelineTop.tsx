import React from "react";

import TimelineLine from "./TimelineLine";

export interface TimelineTopProps {
  height?: number | string;
  children?: React.ReactElement;
}

const TimelineTop: React.FC<TimelineTopProps> = ({ height, children }) => {
  return (
    <div style={{ height: height }} className="timeline-top">
      <TimelineLine center={null} />
      {children}
    </div>
  );
};

export default TimelineTop;
