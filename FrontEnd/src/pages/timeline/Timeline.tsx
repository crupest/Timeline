import * as React from "react";
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

import { useUser } from "@/services/user";
import { getTimelinePostUpdate$ } from "@/services/timeline";

import TimelinePostListView from "./TimelinePostListView";
import TimelineEmptyItem from "./TimelineEmptyItem";
import TimelineLoading from "./TimelineLoading";
import TimelinePostEdit from "./TimelinePostEdit";
import TimelinePostEditNoLogin from "./TimelinePostEditNoLogin";
import TimelineCard from "./TimelineCard";

import "./Timeline.css";

export interface TimelineProps {
  className?: string;
  style?: React.CSSProperties;
  timelineOwner: string;
  timelineName: string;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const { timelineOwner, timelineName, className, style } = props;

  const user = useUser();

  const [timeline, setTimeline] = React.useState<HttpTimelineInfo | null>(null);
  const [posts, setPosts] = React.useState<HttpTimelinePostInfo[] | null>(null);
  const [signalrState, setSignalrState] = React.useState<HubConnectionState>(
    HubConnectionState.Connecting
  );
  const [error, setError] = React.useState<
    "offline" | "forbid" | "notfound" | "error" | null
  >(null);

  const [currentPage, setCurrentPage] = React.useState(1);
  const [totalPage, setTotalPage] = React.useState(0);

  const [timelineReloadKey, setTimelineReloadKey] = React.useState(0);
  const [postsReloadKey, setPostsReloadKey] = React.useState(0);

  const updateTimeline = (): void => setTimelineReloadKey((o) => o + 1);
  const updatePosts = (): void => setPostsReloadKey((o) => o + 1);

  React.useEffect(() => {
    setTimeline(null);
    setPosts(null);
    setError(null);
    setSignalrState(HubConnectionState.Connecting);
  }, [timelineOwner, timelineName]);

  React.useEffect(() => {
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
        }
      );
  }, [timelineOwner, timelineName, timelineReloadKey]);

  React.useEffect(() => {
    getHttpTimelineClient()
      .listPost(timelineOwner, timelineName, 1)
      .then(
        (page) => {
          setPosts(
            page.items.filter((p): p is HttpTimelinePostInfo => !p.deleted)
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
        }
      );
  }, [timelineOwner, timelineName, postsReloadKey]);

  React.useEffect(() => {
    const timelinePostUpdate$ = getTimelinePostUpdate$(
      timelineOwner,
      timelineName
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
            (p): p is HttpTimelinePostInfo => !p.deleted
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
        }
      );
  }, currentPage < totalPage);

  if (error === "offline") {
    return (
      <div className={className} style={style}>
        Offline.
      </div>
    );
  } else if (error === "notfound") {
    return (
      <div className={className} style={style}>
        Not exist.
      </div>
    );
  } else if (error === "forbid") {
    return (
      <div className={className} style={style}>
        Forbid.
      </div>
    );
  } else if (error === "error") {
    return (
      <div className={className} style={style}>
        Error.
      </div>
    );
  }
  return (
    <>
      {timeline == null && posts == null && <TimelineLoading />}
      {timeline && (
        <TimelineCard
          className="timeline-card"
          timeline={timeline}
          connectionStatus={signalrState}
          onReload={updateTimeline}
        />
      )}
      {posts && (
        <div style={style} className={classnames("timeline", className)}>
          <TimelineEmptyItem className="timeline-top" height={50} />
          {timeline?.postable ? (
            <TimelinePostEdit timeline={timeline} onPosted={updatePosts} />
          ) : user == null ? (
            <TimelinePostEditNoLogin />
          ) : null}
          <TimelinePostListView posts={posts} onReload={updatePosts} />
        </div>
      )}
    </>
  );
};

export default Timeline;
