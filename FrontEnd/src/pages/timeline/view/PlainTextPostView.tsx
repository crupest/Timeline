import { useEffect, useState } from "react";
import classNames from "classnames";

import {
  HttpTimelinePostInfo,
  getHttpTimelineClient,
} from "~src/http/timeline";

import { subscribePromise } from "~src/components/utilities";
import Skeleton from "~src/components/Skeleton";

import "./PlainTextPostView.css";

interface PlainTextPostViewProps {
  post?: HttpTimelinePostInfo;
  className?: string;
}

export default function PlainTextPostView({
  post,
  className,
}: PlainTextPostViewProps) {
  const [text, setText] = useState<string | null>(null);

  useEffect(() => {
    if (post) {
      return subscribePromise(
        getHttpTimelineClient().getPostDataAsString(
          post.timelineOwnerV2,
          post.timelineNameV2,
          post.id,
        ),
        setText,
      );
    }
  }, [post]);

  return (
    <div
      className={classNames("timeline-view-plain-text-container", className)}
    >
      {text == null ? (
        <Skeleton />
      ) : (
        <div className="timeline-view-plain-text">{text}</div>
      )}
    </div>
  );
}
