import React from "react";
import classnames from "classnames";

export interface TimelineLineProps {
  current?: boolean;
  startSegmentLength?: string | number;
  center: "node" | "loading" | "none";
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
    <div
      className={classnames(
        "timeline-line",
        current && "current",
        center === "loading" && "loading",
        className
      )}
      style={style}
    >
      <div className="segment start" style={{ height: startSegmentLength }} />
      {center !== "none" ? (
        <div className="node-container">
          <div className="node"></div>
          {center === "loading" ? (
            <svg className="node-loading-edge" viewBox="0 0 100 100">
              <path
                d="M 50,10 A 40 40 45 0 1 78.28,21.72"
                stroke="currentcolor"
                strokeLinecap="square"
                strokeWidth="8"
              />
            </svg>
          ) : null}
        </div>
      ) : null}
      {center !== "loading" ? <div className="segment end"></div> : null}
      {current && <div className="segment current-end" />}
    </div>
  );
};

export default TimelineLine;
