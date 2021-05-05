import React from "react";
import { fromEvent } from "rxjs";
import { filter, throttleTime } from "rxjs/operators";

import { HttpTimelinePostInfo } from "@/http/timeline";

import TimelinePostListView from "./TimelinePostListView";
import { setRecordDisabled } from "@/utilities/useReverseScrollPositionRemember";

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

  const scrollTopHandler = React.useRef<() => void>();

  React.useEffect(() => {
    scrollTopHandler.current = () => {
      if (lastViewCount < posts.length) {
        setRecordDisabled(true);
        setLastViewCount(lastViewCount + 10);
        setTimeout(() => {
          setRecordDisabled(false);
        }, 500);
      }
    };
  }, [lastViewCount, posts]);

  React.useEffect(() => {
    const subscription = fromEvent(window, "scroll")
      .pipe(
        filter(() => window.scrollY === 0),
        throttleTime(800)
      )
      .subscribe(() => {
        scrollTopHandler.current?.();
      });
    return () => subscription.unsubscribe();
  }, []);

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
