import React from "react";

import { HttpTimelinePostInfo } from "@/http/timeline";

import useScrollToTop from "@/utilities/useScrollToTop";

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

  useScrollToTop(() => {
    setLastViewCount(lastViewCount + 10);
  }, lastViewCount < posts.length);

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
