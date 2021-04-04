import React from "react";

import {
  HttpForbiddenError,
  HttpNetworkError,
  HttpNotFoundError,
} from "@/http/common";
import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import TimelinePagedPostListView from "./TimelinePagedPostListView";
import TimelineTop from "./TimelineTop";

export interface TimelineProps {
  className?: string;
  style?: React.CSSProperties;
  top?: string | number;
  timelineName: string;
  reloadKey: number;
  onReload: () => void;
  onLoad?: () => void;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const {
    timelineName,
    className,
    style,
    top,
    reloadKey,
    onReload,
    onLoad,
  } = props;

  const [posts, setPosts] = React.useState<
    | HttpTimelinePostInfo[]
    | "loading"
    | "offline"
    | "notexist"
    | "forbid"
    | "error"
  >("loading");

  React.useEffect(() => {
    setPosts("loading");
  }, [timelineName]);

  React.useEffect(() => {
    let subscribe = true;

    void getHttpTimelineClient()
      .listPost(timelineName)
      .then(
        (data) => {
          if (subscribe) setPosts(data);
        },
        (error) => {
          if (error instanceof HttpNetworkError) {
            setPosts("offline");
          } else if (error instanceof HttpForbiddenError) {
            setPosts("forbid");
          } else if (error instanceof HttpNotFoundError) {
            setPosts("notexist");
          } else {
            console.error(error);
            setPosts("error");
          }
        }
      );

    return () => {
      subscribe = false;
    };
  }, [timelineName, reloadKey]);

  React.useEffect(() => {
    if (Array.isArray(posts)) {
      onLoad?.();
    }
  }, [posts, onLoad]);

  switch (posts) {
    case "loading":
      return (
        <div>
          <TimelineTop
            lineProps={{
              center: "loading",
              startSegmentLength: 56,
            }}
          />
        </div>
      );
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
        <TimelinePagedPostListView
          posts={posts}
          top={top}
          onReload={onReload}
        />
      );
  }
};

export default Timeline;
