import { useEffect, useMemo, useState } from "react";
import { marked } from "marked";
import classNames from "classnames";

import {
  HttpTimelinePostInfo,
  getHttpTimelineClient,
} from "~src/http/timeline";

import { subscribePromise } from "~src/components/utilities";
import Skeleton from "~src/components/Skeleton";

import "./MarkdownPostView.css";

interface MarkdownPostViewProps {
  post?: HttpTimelinePostInfo;
  className?: string;
}

export default function MarkdownPostView({
  post,
  className,
}: MarkdownPostViewProps) {
  const [markdown, setMarkdown] = useState<string | null>(null);

  useEffect(() => {
    if (post) {
      return subscribePromise(
        getHttpTimelineClient().getPostDataAsString(
          post.timelineOwnerV2,
          post.timelineNameV2,
          post.id,
        ),
        setMarkdown,
      );
    }
  }, [post]);

  const markdownHtml = useMemo<string | null>(() => {
    if (markdown == null) return null;
    return marked.parse(markdown, {
      async: false,
    });
  }, [markdown]);

  return (
    <div className={classNames("timeline-view-markdown-container", className)}>
      {markdownHtml == null ? (
        <Skeleton />
      ) : (
        <div
          className="timeline-view-markdown"
          dangerouslySetInnerHTML={{ __html: markdownHtml }}
        />
      )}
    </div>
  );
}
