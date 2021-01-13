import React from "react";
import clsx from "clsx";

import { TimelinePostInfo } from "@/services/timeline";

import TimelineItem from "./TimelineItem";
import TimelineTop from "./TimelineTop";
import TimelineDateItem from "./TimelineDateItem";

function dateEqual(left: Date, right: Date): boolean {
  return (
    left.getDate() == right.getDate() &&
    left.getMonth() == right.getMonth() &&
    left.getFullYear() == right.getFullYear()
  );
}

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

  const groupedPosts = React.useMemo<
    { date: Date; posts: (TimelinePostInfoEx & { index: number })[] }[]
  >(() => {
    const result: {
      date: Date;
      posts: (TimelinePostInfoEx & { index: number })[];
    }[] = [];
    let index = 0;
    for (const post of posts) {
      const { time } = post;
      if (result.length === 0) {
        result.push({ date: time, posts: [{ ...post, index }] });
      } else {
        const lastGroup = result[result.length - 1];
        if (dateEqual(lastGroup.date, time)) {
          lastGroup.posts.push({ ...post, index });
        } else {
          result.push({ date: time, posts: [{ ...post, index }] });
        }
      }
      index++;
    }
    return result;
  }, [posts]);

  return (
    <div
      ref={props.containerRef}
      style={props.style}
      className={clsx("timeline", props.className)}
    >
      <TimelineTop height="56px" />
      {groupedPosts.map((group) => {
        return (
          <>
            <TimelineDateItem date={group.date} />
            {group.posts.map((post) => (
              <TimelineItem
                post={post}
                key={post.id}
                current={posts.length - 1 === post.index}
                more={
                  post.onDelete != null
                    ? {
                        isOpen: showMoreIndex === post.index,
                        toggle: () =>
                          setShowMoreIndex((old) =>
                            old === post.index ? -1 : post.index
                          ),
                        onDelete: post.onDelete,
                      }
                    : undefined
                }
                onClick={() => setShowMoreIndex(-1)}
              />
            ))}
          </>
        );
      })}
    </div>
  );
};

export default Timeline;
