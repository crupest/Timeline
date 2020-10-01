import React from "react";

export interface TimelineTopProps {
  children: React.ReactElement;
}

const TimelineTop: React.FC<TimelineTopProps> = ({ children }) => {
  return (
    <div className="timeline-top">
      <div className="timeline-line-area">
        <div className="timeline-line-segment"></div>
      </div>
      {children}
    </div>
  );
};

export default TimelineTop;
