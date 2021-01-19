import React from "react";
import clsx from "clsx";

import {
  TimelineInfo,
  TimelinePostInfo,
  timelineService,
} from "@/services/timeline";
import { useUser } from "@/services/user";
import { pushAlert } from "@/services/alert";

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

export interface TimelineProps {
  className?: string;
  style?: React.CSSProperties;
  timeline: TimelineInfo;
  posts: TimelinePostInfo[];
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const { timeline, posts } = props;

  const user = useUser();

  const [showMoreIndex, setShowMoreIndex] = React.useState<number>(-1);

  const groupedPosts = React.useMemo<
    { date: Date; posts: (TimelinePostInfo & { index: number })[] }[]
  >(() => {
    const result: {
      date: Date;
      posts: (TimelinePostInfo & { index: number })[];
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
    <div style={props.style} className={clsx("timeline", props.className)}>
      <TimelineTop height="56px" />
      {groupedPosts.map((group) => {
        return (
          <>
            <TimelineDateItem date={group.date} />
            {group.posts.map((post) => {
              const deletable = timelineService.hasModifyPostPermission(
                user,
                timeline,
                post
              );
              return (
                <TimelineItem
                  post={post}
                  key={post.id}
                  current={posts.length - 1 === post.index}
                  more={
                    deletable
                      ? {
                          isOpen: showMoreIndex === post.index,
                          toggle: () =>
                            setShowMoreIndex((old) =>
                              old === post.index ? -1 : post.index
                            ),
                          onDelete: () => {
                            timelineService
                              .deletePost(timeline.name, post.id)
                              .catch(() => {
                                pushAlert({
                                  type: "danger",
                                  message: {
                                    type: "i18n",
                                    key: "timeline.deletePostFailed",
                                  },
                                });
                              });
                          },
                        }
                      : undefined
                  }
                  onClick={() => setShowMoreIndex(-1)}
                />
              );
            })}
          </>
        );
      })}
    </div>
  );
};

export default Timeline;
