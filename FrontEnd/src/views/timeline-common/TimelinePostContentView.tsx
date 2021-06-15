import React from "react";
import classnames from "classnames";
import { Remarkable } from "remarkable";

import { UiLogicError } from "@/common";

import { HttpNetworkError } from "@/http/common";
import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import { useUser } from "@/services/user";

import Skeleton from "../common/Skeleton";
import LoadFailReload from "../common/LoadFailReload";

const TextView: React.FC<TimelinePostContentViewProps> = (props) => {
  const { post, className, style } = props;

  const [text, setText] = React.useState<string | null>(null);
  const [error, setError] = React.useState<"offline" | "error" | null>(null);

  const [reloadKey, setReloadKey] = React.useState<number>(0);

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
  }, [post.timelineName, post.id, reloadKey]);

  if (error != null) {
    return (
      <LoadFailReload
        className={className}
        style={style}
        onReload={() => setReloadKey(reloadKey + 1)}
      />
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
      className={classnames(className, "timeline-content-image")}
      style={style}
    />
  );
};

const MarkdownView: React.FC<TimelinePostContentViewProps> = (props) => {
  const { post, className, style } = props;

  const _remarkable = React.useRef<Remarkable>();

  const getRemarkable = (): Remarkable => {
    if (_remarkable.current) {
      return _remarkable.current;
    } else {
      _remarkable.current = new Remarkable();
      return _remarkable.current;
    }
  };

  const [markdown, setMarkdown] = React.useState<string | null>(null);
  const [error, setError] = React.useState<"offline" | "error" | null>(null);

  const [reloadKey, setReloadKey] = React.useState<number>(0);

  React.useEffect(() => {
    let subscribe = true;

    setMarkdown(null);
    setError(null);

    void getHttpTimelineClient()
      .getPostDataAsString(post.timelineName, post.id)
      .then(
        (data) => {
          if (subscribe) setMarkdown(data);
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
  }, [post.timelineName, post.id, reloadKey]);

  const markdownHtml = React.useMemo<string | null>(() => {
    if (markdown == null) return null;
    return getRemarkable().render(markdown);
  }, [markdown]);

  if (error != null) {
    return (
      <LoadFailReload
        className={className}
        style={style}
        onReload={() => setReloadKey(reloadKey + 1)}
      />
    );
  } else if (markdown == null) {
    return <Skeleton />;
  } else {
    if (markdownHtml == null) {
      throw new UiLogicError("Markdown is not null but markdown html is.");
    }
    return (
      <div
        className={classnames(className, "markdown-container")}
        style={style}
        dangerouslySetInnerHTML={{
          __html: markdownHtml,
        }}
      />
    );
  }
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
    console.error("Unknown post type", post);
    return <div>Error, unknown post type!</div>;
  }
};

export default TimelinePostContentView;
