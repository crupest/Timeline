import React from "react";
import { HubConnectionState } from "@microsoft/signalr";

import {
  HttpForbiddenError,
  HttpNetworkError,
  HttpNotFoundError,
} from "http/common";
import { getHttpTimelineClient, HttpTimelinePostInfo } from "http/timeline";

import { getTimelinePostUpdate$ } from "@/services/timeline";

import TimelinePagedPostListView from "./TimelinePagedPostListView";
import TimelineTop from "./TimelineTop";
import TimelineLoading from "./TimelineLoading";

export interface TimelineProps {
  className?: string;
  style?: React.CSSProperties;
  timelineName?: string;
  reloadKey: number;
  onReload: () => void;
  onConnectionStateChanged?: (state: HubConnectionState) => void;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const { timelineName, className, style, reloadKey } = props;

  const [state, setState] = React.useState<
    "loading" | "loaded" | "offline" | "notexist" | "forbid" | "error"
  >("loading");
  const [posts, setPosts] = React.useState<HttpTimelinePostInfo[]>([]);

  React.useEffect(() => {
    setState("loading");
    setPosts([]);
  }, [timelineName]);

  const onReload = React.useRef<() => void>(props.onReload);

  React.useEffect(() => {
    onReload.current = props.onReload;
  }, [props.onReload]);

  const onConnectionStateChanged = React.useRef<
    ((state: HubConnectionState) => void) | null
  >(null);

  React.useEffect(() => {
    onConnectionStateChanged.current = props.onConnectionStateChanged ?? null;
  }, [props.onConnectionStateChanged]);

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
  }, [timelineName, state]);

  React.useEffect(() => {
    if (timelineName != null) {
      let subscribe = true;

      void getHttpTimelineClient()
        .listPost(timelineName)
        .then(
          (data) => {
            if (subscribe) {
              setState("loaded");
              setPosts(data);
            }
          },
          (error) => {
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
        );

      return () => {
        subscribe = false;
      };
    }
  }, [timelineName, reloadKey]);

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
        <>
          <TimelineTop height={40} />
          <TimelinePagedPostListView
            posts={posts}
            onReload={onReload.current}
          />
        </>
      );
  }
};

export default Timeline;
