import React from "react";
import clsx from "clsx";

import { HttpNetworkError } from "@/http/common";
import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import { useUser } from "@/services/user";

import Skeleton from "../common/Skeleton";

const TextView: React.FC<TimelinePostContentViewProps> = (props) => {
  const { post, className, style } = props;

  const [text, setText] = React.useState<string | null>(null);
  const [error, setError] = React.useState<"offline" | "error" | null>(null);

  React.useEffect(() => {
    let subscribe = true;

    setText(null);
    setError(null);

    void getHttpTimelineClient()
      .getPostDataAsString(post.timelineName, post.id)
      .then(
        (data) => {
          if (subscribe) setText(data);
        },
        (error) => {
          if (subscribe) {
            if (error instanceof HttpNetworkError) {
              setError("offline");
            } else {
              setError("error");
            }
          }
        }
      );

    return () => {
      subscribe = false;
    };
  }, [post.timelineName, post.id]);

  if (error != null) {
    // TODO: i18n
    return (
      <div className={className} style={style}>
        Error!
      </div>
    );
  } else if (text == null) {
    return <Skeleton />;
  } else {
    return (
      <div className={className} style={style}>
        {text}
      </div>
    );
  }
};

const ImageView: React.FC<TimelinePostContentViewProps> = (props) => {
  const { post, className, style } = props;

  useUser();

  return (
    <img
      src={getHttpTimelineClient().generatePostDataUrl(
        post.timelineName,
        post.id
      )}
      className={clsx(className, "timeline-content-image")}
      style={style}
    />
  );
};

const MarkdownView: React.FC<TimelinePostContentViewProps> = (_props) => {
  // TODO: Implement this.
  return <div>Unsupported now!</div>;
};

export interface TimelinePostContentViewProps {
  post: HttpTimelinePostInfo;
  className?: string;
  style?: React.CSSProperties;
}

const viewMap: Record<string, React.FC<TimelinePostContentViewProps>> = {
  "text/plain": TextView,
  "text/markdown": MarkdownView,
  "image/png": ImageView,
  "image/jpeg": ImageView,
  "image/gif": ImageView,
  "image/webp": ImageView,
};

const TimelinePostContentView: React.FC<TimelinePostContentViewProps> = (
  props
) => {
  const { post, className, style } = props;

  const type = post.dataList[0].kind;

  if (type in viewMap) {
    const View = viewMap[type];
    return <View post={post} className={className} style={style} />;
  } else {
    // TODO: i18n
    return <div>Error, unknown post type!</div>;
  }
};

export default TimelinePostContentView;
