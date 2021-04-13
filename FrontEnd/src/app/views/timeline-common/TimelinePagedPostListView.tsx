import React from "react";

import { HttpTimelinePostInfo } from "@/http/timeline";

import TimelinePostListView from "./TimelinePostListView";

export interface TimelinePagedPostListViewProps {
  className?: string;
  style?: React.CSSProperties;
  posts: HttpTimelinePostInfo[];
  onReload: () => void;
}

const TimelinePagedPostListView: React.FC<TimelinePagedPostListViewProps> = (
  props
) => {
  const { className, style, posts, onReload } = props;

  const [lastViewCount, setLastViewCount] = React.useState<number>(10);

  const viewingPosts = React.useMemo(() => {
    return lastViewCount >= posts.length
      ? posts.slice()
      : posts.slice(-lastViewCount);
  }, [posts, lastViewCount]);

  React.useEffect(() => {
    if (lastViewCount < posts.length) {
      const listener = (): void => {
        if (window.scrollY === 0 && lastViewCount < posts.length) {
          setLastViewCount(lastViewCount + 10);
        }
      };
      window.addEventListener("scroll", listener);
      return () => window.removeEventListener("scroll", listener);
    }
  }, [lastViewCount, posts]);

  return (
    <TimelinePostListView
      className={className}
      style={style}
      posts={viewingPosts}
      onReload={onReload}
    />
  );
};

export default TimelinePagedPostListView;
