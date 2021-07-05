import React from "react";
import classnames from "classnames";

import Card from "../common/Card";
import TimelineLine from "./TimelineLine";

import "./TimelinePostEdit.css";

export interface TimelinePostEditCardProps {
  className?: string;
  style?: React.CSSProperties;
  children?: React.ReactNode;
}

const TimelinePostEdit: React.FC<TimelinePostEditCardProps> = ({
  className,
  style,
  children,
}) => {
  return (
    <div
      className={classnames("timeline-item timeline-post-edit", className)}
      style={style}
    >
      <TimelineLine center="node" current />
      <Card className="timeline-item-card">{children}</Card>
    </div>
  );
};

export default TimelinePostEdit;
