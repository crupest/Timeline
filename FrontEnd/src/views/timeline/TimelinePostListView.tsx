import React, { Fragment } from "react";

import { HttpTimelinePostInfo } from "@/http/timeline";

import TimelinePostView from "./TimelinePostView";
import TimelineDateLabel from "./TimelineDateLabel";

function dateEqual(left: Date, right: Date): boolean {
  return (
    left.getDate() == right.getDate() &&
    left.getMonth() == right.getMonth() &&
    left.getFullYear() == right.getFullYear()
  );
}

export interface TimelinePostListViewProps {
  posts: HttpTimelinePostInfo[];
  onReload: () => void;
}

const TimelinePostListView: React.FC<TimelinePostListViewProps> = (props) => {
  const { posts, onReload } = props;

  const groupedPosts = React.useMemo<
    {
      date: Date;
      posts: (HttpTimelinePostInfo & { index: number })[];
    }[]
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
    <>
      {groupedPosts.map((group) => {
        return (
          <Fragment key={group.date.toDateString()}>
            <TimelineDateLabel date={group.date} />
            {group.posts.map((post) => {
              return (
                <TimelinePostView
                  key={post.id}
                  post={post}
                  onChanged={onReload}
                  onDeleted={onReload}
                />
              );
            })}
          </Fragment>
        );
      })}
    </>
  );
};

export default TimelinePostListView;