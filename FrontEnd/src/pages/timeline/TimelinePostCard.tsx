import { ReactNode } from "react";
import classNames from "classnames";

import Card from "@/views/common/Card";

import "./TimelinePostCard.css";

export interface TimelinePostEditCardProps {
  className?: string;
  children?: ReactNode;
}

export default function TimelinePostCard({
  className,
  children,
}: TimelinePostEditCardProps) {
  return (
    <Card className={classNames("timeline-post-card", className)}>
      {children}
    </Card>
  );
}
