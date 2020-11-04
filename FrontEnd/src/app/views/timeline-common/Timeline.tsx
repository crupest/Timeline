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
  posts: TimelinePostInfoEx[];
  onDelete: TimelineDeleteCallback;
  onResize?: () => void;
  containerRef?: React.Ref<HTMLDivElement>;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const { posts, onDelete, onResize } = props;

  const [indexShowDeleteButton, setIndexShowDeleteButton] = React.useState<
    number
  >(-1);

  const onItemClick = React.useCallback(() => {
    setIndexShowDeleteButton(-1);
  }, []);

  const onToggleDelete = React.useMemo(() => {
    return posts.map((post, i) => {
      return post.deletable
        ? () => {
            setIndexShowDeleteButton((oldIndexShowDeleteButton) => {
              return oldIndexShowDeleteButton !== i ? i : -1;
            });
          }
        : undefined;
    });
  }, [posts]);

  const onItemDelete = React.useMemo(() => {
    return posts.map((post, i) => {
      return () => {
        onDelete(i, post.id);
      };
    });
  }, [posts, onDelete]);

  return (
    <div ref={props.containerRef} className={clsx("timeline", props.className)}>
      <TimelineTop height="56px" />
      {(() => {
        const length = posts.length;
        return posts.map((post, i) => {
          const toggleMore = onToggleDelete[i];

          return (
            <TimelineItem
              post={post}
              key={post.id}
              current={length - 1 === i}
              more={
                toggleMore
                  ? {
                      isOpen: indexShowDeleteButton === i,
                      toggle: toggleMore,
                      onDelete: onItemDelete[i],
                    }
                  : undefined
              }
              onClick={onItemClick}
              onResize={onResize}
            />
          );
        });
      })()}
    </div>
  );
};

export default Timeline;
