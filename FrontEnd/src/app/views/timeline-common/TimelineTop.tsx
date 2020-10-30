import React from "react";

export interface TimelineTopProps {
  height?: number | string;
  children?: React.ReactElement;
}

const TimelineTop: React.FC<TimelineTopProps> = ({ height, children }) => {
  return (
    <div style={{ height: height }} className="timeline-top">
      <div className="timeline-line-area-container">
        <div className="timeline-line-area">
          <div className="timeline-line-segment"></div>
        </div>
      </div>
      {children}
    </div>
  );
};

export default TimelineTop;
