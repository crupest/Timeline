import React from "react";
import { HubConnectionState } from "@microsoft/signalr";
import classnames from "classnames";

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
import { useUser } from "@/services/user";

import useValueWithRef from "@/utilities/useValueWithRef";

import TimelinePagedPostListView from "./TimelinePagedPostListView";
import TimelineEmptyItem from "./TimelineEmptyItem";
import TimelineLoading from "./TimelineLoading";
import TimelinePostEdit from "./TimelinePostEdit";
import TimelinePostEditNoLogin from "./TimelinePostEditNoLogin";

import "./index.css";

export interface TimelineProps {
  className?: string;
  style?: React.CSSProperties;
  timelineName: string;
  reloadKey: number;
  onReload: () => void;
  onTimelineLoaded?: (timeline: HttpTimelineInfo) => void;
  onConnectionStateChanged?: (state: HubConnectionState) => void;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const { timelineName, className, style, reloadKey } = props;

  const user = useUser();

  const [state, setState] = React.useState<
    "loading" | "loaded" | "offline" | "notexist" | "forbid" | "error"
  >("loading");
  const [timeline, setTimeline] = React.useState<HttpTimelineInfo | null>(null);
  const [posts, setPosts] = React.useState<HttpTimelinePostInfo[]>([]);

  React.useEffect(() => {
    setState("loading");
    setTimeline(null);
    setPosts([]);
  }, [timelineName]);

  const onReload = useValueWithRef(props.onReload);
  const onTimelineLoaded = useValueWithRef(props.onTimelineLoaded);
  const onConnectionStateChanged = useValueWithRef(
    props.onConnectionStateChanged
  );

  React.useEffect(() => {
    if (timelineName != null && state === "loaded") {
      const timelinePostUpdate$ = getTimelinePostUpdate$(timelineName);
      const subscription = timelinePostUpdate$.subscribe(
        ({ update, state }) => {
          if (update) {
            onReload.current();
          }
          onConnectionStateChanged.current?.(state);
        }
      );
      return () => {
        subscription.unsubscribe();
      };
    }
  }, [timelineName, state, onReload, onConnectionStateChanged]);

  React.useEffect(() => {
    if (timelineName != null) {
      let subscribe = true;

      const client = getHttpTimelineClient();
      Promise.all([
        client.getTimeline(timelineName),
        client.listPost(timelineName),
      ]).then(
        ([t, p]) => {
          if (subscribe) {
            setTimeline(t);
            setPosts(p);
            setState("loaded");
            onTimelineLoaded.current?.(t);
          }
        },
        (error) => {
          if (subscribe) {
            if (error instanceof HttpNetworkError) {
              setState("offline");
            } else if (error instanceof HttpForbiddenError) {
              setState("forbid");
            } else if (error instanceof HttpNotFoundError) {
              setState("notexist");
            } else {
              console.error(error);
              setState("error");
            }
          }
        }
      );

      return () => {
        subscribe = false;
      };
    }
  }, [timelineName, reloadKey, onTimelineLoaded]);

  switch (state) {
    case "loading":
      return <TimelineLoading />;
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
      return (
        <div style={style} className={classnames("timeline", className)}>
          <TimelineEmptyItem height={40} />
          <TimelinePagedPostListView
            posts={posts}
            onReload={onReload.current}
          />
          {timeline?.postable ? (
            <TimelinePostEdit timeline={timeline} onPosted={onReload.current} />
          ) : user == null ? (
            <TimelinePostEditNoLogin />
          ) : (
            <TimelineEmptyItem startSegmentLength={20} center="none" current />
          )}
        </div>
      );
  }
};

export default Timeline;
