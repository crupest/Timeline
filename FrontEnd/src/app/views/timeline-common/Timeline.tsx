import React from "react";
import clsx from "clsx";

import { TimelinePostInfo } from "@/services/timeline";

import TimelineItem from "./TimelineItem";
import TimelineTop from "./TimelineTop";

export interface TimelinePostInfoEx extends TimelinePostInfo {
  onDelete?: () => void;
}

export type TimelineDeleteCallback = (index: number, id: number) => void;

export interface TimelineProps {
  className?: string;
  style?: React.CSSProperties;
  posts: TimelinePostInfoEx[];
  containerRef?: React.Ref<HTMLDivElement>;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const { posts } = props;

  const [showMoreIndex, setShowMoreIndex] = React.useState<number>(-1);

  return (
    <div
      ref={props.containerRef}
      style={props.style}
      className={clsx("timeline", props.className)}
    >
      <TimelineTop height="56px" />
      {(() => {
        const length = posts.length;
        return posts.map((post, index) => {
          return (
            <TimelineItem
              post={post}
              key={post.id}
              current={length - 1 === index}
              more={
                post.onDelete != null
                  ? {
                      isOpen: showMoreIndex === index,
                      toggle: () =>
                        setShowMoreIndex((old) => (old === index ? -1 : index)),
                      onDelete: post.onDelete,
                    }
                  : undefined
              }
              onClick={() => setShowMoreIndex(-1)}
            />
          );
        });
      })()}
    </div>
  );
};

export default Timeline;
