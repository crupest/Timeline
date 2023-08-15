import { useState, useEffect } from "react";
import classnames from "classnames";
import { useScrollToBottom } from "@/utilities/hooks";
import { HubConnectionState } from "@microsoft/signalr";

import {
  HttpForbiddenError,
  HttpNetworkError,
  HttpNotFoundError,
} from "@/http/common";
import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePostInfo,
} from "@/http/timeline";

import { getTimelinePostUpdate$ } from "@/services/timeline";

import TimelinePostList from "./TimelinePostList";
import TimelinePostEdit from "./TimelinePostCreateView";
import TimelineCard from "./TimelineCard";

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

  const [timelineReloadKey, setTimelineReloadKey] = useState(0);
  const [postsReloadKey, setPostsReloadKey] = useState(0);

  const updateTimeline = (): void => setTimelineReloadKey((o) => o + 1);
  const updatePosts = (): void => setPostsReloadKey((o) => o + 1);

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
  }, [timelineOwner, timelineName, timelineReloadKey]);

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
  }, [timelineOwner, timelineName, postsReloadKey]);

  useEffect(() => {
    const timelinePostUpdate$ = getTimelinePostUpdate$(
      timelineOwner,
      timelineName,
    );
    const subscription = timelinePostUpdate$.subscribe(({ update, state }) => {
      if (update) {
        setPostsReloadKey((o) => o + 1);
      }
      setSignalrState(state);
    });
    return () => {
      subscription.unsubscribe();
    };
  }, [timelineOwner, timelineName]);

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
        <TimelineCard
          timeline={timeline}
          connectionStatus={signalrState}
          onReload={updateTimeline}
        />
      )}
      {posts && (
        <div className={classnames("timeline", className)}>
          {timeline?.postable && (
            <TimelinePostEdit timeline={timeline} onPosted={updatePosts} />
          )}
          <TimelinePostList posts={posts} onReload={updatePosts} />
        </div>
      )}
    </div>
  );
}

export default Timeline;
