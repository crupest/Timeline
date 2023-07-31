import { ReactNode } from "react";
import classNames from "classnames";

import "./TimelinePostContainer.css";

export interface TimelinePostEditCardProps {
  className?: string;
  children?: ReactNode;
}

export default function TimelinePostContainer({
  className,
  children,
}: TimelinePostEditCardProps) {
  return (
    <div className={classNames("timeline-post-container", className)}>
      {children}
    </div>
  );
}
