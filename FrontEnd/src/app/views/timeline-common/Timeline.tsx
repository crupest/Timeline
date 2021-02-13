import React from "react";
import clsx from "clsx";

import {
  HttpForbiddenError,
  HttpNetworkError,
  HttpNotFoundError,
} from "@/http/common";
import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import TimelineItem from "./TimelineItem";
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
  timelineName: string;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const { timelineName, className, style } = props;

  const [posts, setPosts] = React.useState<
    | HttpTimelinePostInfo[]
    | "loading"
    | "offline"
    | "notexist"
    | "forbid"
    | "error"
  >("loading");

  React.useEffect(() => {
    let subscribe = true;

    void getHttpTimelineClient()
      .listPost(timelineName)
      .then(
        (data) => {
          if (subscribe) setPosts(data);
        },
        (error) => {
          if (error instanceof HttpNetworkError) {
            setPosts("offline");
          } else if (error instanceof HttpForbiddenError) {
            setPosts("forbid");
          } else if (error instanceof HttpNotFoundError) {
            setPosts("notexist");
          } else {
            console.error(error);
            setPosts("error");
          }
        }
      );

    return () => {
      subscribe = false;
    };
  }, [timelineName]);

  switch (posts) {
    case "loading":
      return (
        <div className={className} style={style}>
          Loading.
        </div>
      );
    case "offline":
      return (
        <div className={className} style={style}>
          Offline.
        </div>
      );
    case "notexist":
      return (
        <div className={className} style={style}>
          Not exist.
        </div>
      );
    case "forbid":
      return (
        <div className={className} style={style}>
          Forbid.
        </div>
      );
    case "error":
      return (
        <div className={className} style={style}>
          Error.
        </div>
      );
    default:
      return <TimelinePostListView posts={posts} />;
  }
};

export interface TimelinePostListViewProps {
  className?: string;
  style?: React.CSSProperties;
  posts: HttpTimelinePostInfo[];
}

export const TimelinePostListView: React.FC<TimelinePostListViewProps> = (
  props
) => {
  const { className, style, posts } = props;

  const groupedPosts = React.useMemo<
    { date: Date; posts: (HttpTimelinePostInfo & { index: number })[] }[]
  >(() => {
    const result: {
      date: Date;
      posts: (HttpTimelinePostInfo & { index: number })[];
    }[] = [];
    let index = 0;
    for (const post of posts) {
      const time = new Date(post.time);
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
    <div style={style} className={clsx("timeline", className)}>
      {groupedPosts.map((group) => {
        return (
          <>
            <TimelineDateItem date={group.date} />
            {group.posts.map((post) => {
              return (
                <TimelineItem
                  key={post.id}
                  post={post}
                  current={posts.length - 1 === post.index}
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
