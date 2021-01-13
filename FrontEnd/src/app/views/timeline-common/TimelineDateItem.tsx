import React from "react";
import TimelineLine from "./TimelineLine";

export interface TimelineDateItemProps {
  date: Date;
}

const TimelineDateItem: React.FC<TimelineDateItemProps> = ({ date }) => {
  return (
    <div className="timeline-date-item">
      <TimelineLine center={null} />
      <div className="timeline-date-item-badge">
        {date.toLocaleDateString()}
      </div>
    </div>
  );
};

export default TimelineDateItem;
