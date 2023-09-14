import { useEffect, useState } from "react";
import classNames from "classnames";

import {
  HttpTimelinePostInfo,
  getHttpTimelineClient,
} from "~src/http/timeline";

import "./ImagePostView.css";

interface ImagePostViewProps {
  post?: HttpTimelinePostInfo;
  className?: string;
}

export default function ImagePostView({ post, className }: ImagePostViewProps) {
  const [url, setUrl] = useState<string | null>(null);

  useEffect(() => {
    if (post) {
      setUrl(
        getHttpTimelineClient().generatePostDataUrl(
          post.timelineOwnerV2,
          post.timelineNameV2,
          post.id,
        ),
      );
    } else {
      setUrl(null);
    }
  }, [post]);

  return (
    <div className={classNames("timeline-view-image-container", className)}>
      <img src={url ?? undefined} className="timeline-view-image" />
    </div>
  );
}
