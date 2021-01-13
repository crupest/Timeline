import clsx from "clsx";
import React from "react";

export interface TimelineLineProps {
  current?: boolean;
  startSegmentLength?: string | number;
  center: "node" | null;
  className?: string;
  style?: React.CSSProperties;
}

const TimelineLine: React.FC<TimelineLineProps> = ({
  startSegmentLength,
  center,
  current,
  className,
  style,
}) => {
  return (
    <div className={clsx("timeline-line", className)} style={style}>
      <div className="segment start" style={{ height: startSegmentLength }} />
      {center == "node" ? (
        <div className="node-container">
          <div className="node"></div>
        </div>
      ) : null}
      <div className="segment end"></div>
      {current && <div className="segment current-end" />}
    </div>
  );
};

export default TimelineLine;
