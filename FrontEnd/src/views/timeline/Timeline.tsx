import React from "react";
import classnames from "classnames";
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

import TimelinePagedPostListView from "./TimelinePagedPostListView";
import TimelineEmptyItem from "./TimelineEmptyItem";
import TimelineLoading from "./TimelineLoading";
import TimelinePostEdit from "./TimelinePostEdit";
import TimelinePostEditNoLogin from "./TimelinePostEditNoLogin";
import TimelineCard from "./TimelineCard";

import "./index.css";

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
    if (timelineName != null) {
      let subscribe = true;

      getHttpTimelineClient()
        .getTimeline(timelineOwner, timelineName)
        .then(
          (t) => {
            if (subscribe) {
              setTimeline(t);
            }
          },
          (error) => {
            if (subscribe) {
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
          }
        );

      return () => {
        subscribe = false;
      };
    }
  }, [timelineOwner, timelineName, timelineReloadKey]);

  React.useEffect(() => {
    let subscribe = true;
    void getHttpTimelineClient()
      .listPost(timelineOwner, timelineName)
      .then(
        (ps) => {
          if (subscribe) {
            setPosts(
              ps.items.filter(
                (p): p is HttpTimelinePostInfo => p.deleted === false
              )
            );
          }
        },
        (error) => {
          if (subscribe) {
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
        }
      );
    return () => {
      subscribe = false;
    };
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
          <TimelineEmptyItem height={40} />
          <TimelinePagedPostListView posts={posts} onReload={updatePosts} />
          {timeline?.postable ? (
            <TimelinePostEdit timeline={timeline} onPosted={updatePosts} />
          ) : user == null ? (
            <TimelinePostEditNoLogin />
          ) : (
            <TimelineEmptyItem startSegmentLength={20} center="none" current />
          )}
        </div>
      )}
    </>
  );
};

export default Timeline;
