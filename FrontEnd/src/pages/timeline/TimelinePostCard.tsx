import { ReactNode } from "react";
import classNames from "classnames";

import Card from "~src/components/Card";

import "./TimelinePostCard.css";

interface TimelinePostCardProps {
  className?: string;
  children?: ReactNode;
}

export default function TimelinePostCard({
  className,
  children,
}: TimelinePostCardProps) {
  return (
    <Card color="primary" className={classNames("timeline-post-card", className)}>
      {children}
    </Card>
  );
}
