import React from "react";
import classnames from "classnames";

import TimelineLine, { TimelineLineProps } from "./TimelineLine";

export interface TimelineEmptyItemProps extends Partial<TimelineLineProps> {
  height?: number | string;
  className?: string;
  style?: React.CSSProperties;
}

const TimelineEmptyItem: React.FC<TimelineEmptyItemProps> = (props) => {
  const { height, style, className, center, ...lineProps } = props;

  return (
    <div
      style={{ ...style, height: height }}
      className={classnames("timeline-item", className)}
    >
      <TimelineLine center={center ?? "none"} {...lineProps} />
    </div>
  );
};

export default TimelineEmptyItem;
