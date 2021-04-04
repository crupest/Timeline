import React from "react";
import clsx from "clsx";

import TimelineLine, { TimelineLineProps } from "./TimelineLine";

export interface TimelineTopProps {
  height?: number | string;
  lineProps?: TimelineLineProps;
  className?: string;
  style?: React.CSSProperties;
}

const TimelineTop: React.FC<TimelineTopProps> = (props) => {
  const { height, style, className } = props;
  const lineProps = props.lineProps ?? { center: "none" };

  return (
    <div
      style={{ ...style, height: height }}
      className={clsx("timeline-top", className)}
    >
      <TimelineLine {...lineProps} />
    </div>
  );
};

export default TimelineTop;
