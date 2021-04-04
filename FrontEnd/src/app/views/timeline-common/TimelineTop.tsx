import React from "react";

import TimelineLine, { TimelineLineProps } from "./TimelineLine";

export interface TimelineTopProps {
  height?: number | string;
  lineProps?: TimelineLineProps;
  children?: React.ReactElement;
}

const TimelineTop: React.FC<TimelineTopProps> = (props) => {
  const { height, children } = props;
  const lineProps = props.lineProps ?? { center: "none" };

  return (
    <div style={{ height: height }} className="timeline-top">
      <TimelineLine {...lineProps} />
      {children}
    </div>
  );
};

export default TimelineTop;
