import React from "react";
import { fromEvent } from "rxjs";

import { HttpTimelinePostInfo } from "@/http/timeline";

import {
  getReverseScrollPosition,
  scrollToReverseScrollPosition,
} from "@/utilities/useReverseScrollPositionRemember";

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

  const lastScrollPosition = React.useRef<number | null>(null);

  React.useEffect(() => {
    if (lastScrollPosition.current != null) {
      scrollToReverseScrollPosition(lastScrollPosition.current);
      lastScrollPosition.current = null;
    }

    if (lastViewCount < posts.length) {
      const subscription = fromEvent(window, "scroll").subscribe(() => {
        if (window.scrollY === 0) {
          lastScrollPosition.current = getReverseScrollPosition();
          setLastViewCount(lastViewCount + 10);
        }
      });
      return () => subscription.unsubscribe();
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
