import { useState, useEffect } from "react";
import classNames from "classnames";
import { HubConnectionState } from "@microsoft/signalr";

import {
  HttpForbiddenError,
  HttpNetworkError,
  HttpNotFoundError,
} from "~src/http/common";
import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePostInfo,
} from "~src/http/timeline";

import { getTimelinePostUpdate$ } from "~src/services/timeline";

import { useReloadKey, useScrollToBottom } from "~src/components/hooks";

import TimelinePostList from "./TimelinePostList";
import TimelineInfoCard from "./TimelineInfoCard";
import TimelinePostEdit from "./edit/TimelinePostCreateView";

import "./Timeline.css";

export interface TimelineProps {
  className?: string;
  timelineOwner: string;
  timelineName: string;
}

export function Timeline(props: TimelineProps) {
  const { timelineOwner, timelineName, className } = props;

  const [timeline, setTimeline] = useState<HttpTimelineInfo | null>(null);
  const [posts, setPosts] = useState<HttpTimelinePostInfo[] | null>(null);
  const [signalrState, setSignalrState] = useState<HubConnectionState>(
    HubConnectionState.Connecting,
  );
  const [error, setError] = useState<
    "offline" | "forbid" | "notfound" | "error" | null
  >(null);

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPage, setTotalPage] = useState(0);

  const [timelineKey, reloadTimeline] = useReloadKey();
  const [postsKey, reloadPosts] = useReloadKey();

  useEffect(() => {
    setTimeline(null);
    setPosts(null);
    setError(null);
    setSignalrState(HubConnectionState.Connecting);
  }, [timelineOwner, timelineName]);

  useEffect(() => {
    getHttpTimelineClient()
      .getTimeline(timelineOwner, timelineName)
      .then(
        (t) => {
          setTimeline(t);
        },
        (error) => {
          if (error instanceof HttpNetworkError) {
            setError("offline");
          } else if (error instanceof HttpForbiddenError) {
            setError("forbid");
          } else if (error instanceof HttpNotFoundError) {
            setError("notfound");
          } else {
            console.error(error);
            setError("error");
          }
        },
      );
  }, [timelineOwner, timelineName, timelineKey]);

  useEffect(() => {
    getHttpTimelineClient()
      .listPost(timelineOwner, timelineName, 1)
      .then(
        (page) => {
          setPosts(
            page.items.filter((p): p is HttpTimelinePostInfo => !p.deleted),
          );
          setTotalPage(page.totalPageCount);
        },
        (error) => {
          if (error instanceof HttpNetworkError) {
            setError("offline");
          } else if (error instanceof HttpForbiddenError) {
            setError("forbid");
          } else if (error instanceof HttpNotFoundError) {
            setError("notfound");
          } else {
            console.error(error);
            setError("error");
          }
        },
      );
  }, [timelineOwner, timelineName, postsKey]);

  useEffect(() => {
    const timelinePostUpdate$ = getTimelinePostUpdate$(
      timelineOwner,
      timelineName,
    );
    const subscription = timelinePostUpdate$.subscribe(({ update, state }) => {
      if (update) {
        reloadPosts();
      }
      setSignalrState(state);
    });
    return () => {
      subscription.unsubscribe();
    };
  }, [timelineOwner, timelineName, reloadPosts]);

  useScrollToBottom(() => {
    console.log(`Load page ${currentPage + 1}.`);
    setCurrentPage(currentPage + 1);
    void getHttpTimelineClient()
      .listPost(timelineOwner, timelineName, currentPage + 1)
      .then(
        (page) => {
          const ps = page.items.filter(
            (p): p is HttpTimelinePostInfo => !p.deleted,
          );
          setPosts((old) => [...(old ?? []), ...ps]);
        },
        (error) => {
          if (error instanceof HttpNetworkError) {
            setError("offline");
          } else if (error instanceof HttpForbiddenError) {
            setError("forbid");
          } else if (error instanceof HttpNotFoundError) {
            setError("notfound");
          } else {
            console.error(error);
            setError("error");
          }
        },
      );
  }, currentPage < totalPage);

  if (error === "offline") {
    return <div className={className}>Offline.</div>;
  } else if (error === "notfound") {
    return <div className={className}>Not exist.</div>;
  } else if (error === "forbid") {
    return <div className={className}>Forbid.</div>;
  } else if (error === "error") {
    return <div className={className}>Error.</div>;
  }
  return (
    <div className="timeline-container">
      {timeline && (
        <TimelineInfoCard
          timeline={timeline}
          connectionStatus={signalrState}
          onReload={reloadTimeline}
        />
      )}
      {posts && (
        <div className={classNames("timeline", className)}>
          {timeline?.postable && (
            <TimelinePostEdit timeline={timeline} onPosted={reloadPosts} />
          )}
          <TimelinePostList posts={posts} onReload={reloadPosts} />
        </div>
      )}
    </div>
  );
}

export default Timeline;
