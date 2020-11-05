import React from "react";
import clsx from "clsx";

import { TimelinePostInfo } from "@/services/timeline";

import TimelineItem from "./TimelineItem";
import TimelineTop from "./TimelineTop";

export interface TimelinePostInfoEx extends TimelinePostInfo {
  deletable: boolean;
}

export type TimelineDeleteCallback = (index: number, id: number) => void;

export interface TimelineProps {
  className?: string;
  style?: React.CSSProperties;
  posts: TimelinePostInfoEx[];
  onDelete: TimelineDeleteCallback;
  onResize?: () => void;
  containerRef?: React.Ref<HTMLDivElement>;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const { posts, onDelete, onResize } = props;

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
                post.deletable
                  ? {
                      isOpen: showMoreIndex === index,
                      toggle: () =>
                        setShowMoreIndex((old) => (old === index ? -1 : index)),
                      onDelete: () => onDelete(index, post.id),
                    }
                  : undefined
              }
              onClick={() => setShowMoreIndex(-1)}
              onResize={onResize}
            />
          );
        });
      })()}
    </div>
  );
};

export default Timeline;
