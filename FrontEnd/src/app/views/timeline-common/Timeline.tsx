import React from "react";

import {
  HttpForbiddenError,
  HttpNetworkError,
  HttpNotFoundError,
} from "@/http/common";
import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import TimelinePostListView from "./TimelinePostListView";

export interface TimelineProps {
  className?: string;
  style?: React.CSSProperties;
  timelineName: string;
  reloadKey: number;
  onReload: () => void;
  additionalPosts?: HttpTimelinePostInfo[];
  onLoad?: () => void;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const {
    timelineName,
    className,
    style,
    reloadKey,
    onReload,
    additionalPosts,
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
    let subscribe = true;

    setPosts("loading");

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
  }, [posts, additionalPosts, onLoad]);

  switch (posts) {
    case "loading":
      return (
        <div className={className} style={style}>
          Loading.
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
        <TimelinePostListView
          posts={[...posts, ...(additionalPosts ?? [])]}
          onReload={onReload}
        />
      );
  }
};

export default Timeline;
