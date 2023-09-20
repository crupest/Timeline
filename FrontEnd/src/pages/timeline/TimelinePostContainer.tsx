import { ReactNode } from "react";
import classNames from "classnames";

import "./TimelinePostContainer.css";

interface TimelinePostContainerProps {
  className?: string;
  children?: ReactNode;
}

export default function TimelinePostContainer({
  className,
  children,
}: TimelinePostContainerProps) {
  return (
    <div className={classNames("timeline-post-container", className)}>
      {children}
    </div>
  );
}
