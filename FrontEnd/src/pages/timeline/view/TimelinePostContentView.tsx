import ImagePostView from "./ImagePostView";
import MarkdownPostView from "./MarkdownPostView";
import PlainTextPostView from "./PlainTextPostView";

import type { HttpTimelinePostInfo } from "~src/http/timeline";

interface TimelinePostContentViewProps {
  post?: HttpTimelinePostInfo;
  className?: string;
}

const viewMap: Record<string, React.FC<TimelinePostContentViewProps>> = {
  "text/plain": PlainTextPostView,
  "text/markdown": MarkdownPostView,
  "image/png": ImagePostView,
  "image/jpeg": ImagePostView,
  "image/gif": ImagePostView,
  "image/webp": ImagePostView,
};

export default function TimelinePostContentView({
  post,
  className,
}: TimelinePostContentViewProps) {
  if (post == null) {
    return <div />;
  }

  const type = post.dataList[0].kind;

  if (type in viewMap) {
    const View = viewMap[type];
    return <View post={post} className={className} />;
  }

  return <div>Unknown post type.</div>;
}
