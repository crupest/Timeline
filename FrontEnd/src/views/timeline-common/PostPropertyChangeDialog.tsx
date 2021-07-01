import React from "react";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import OperationDialog from "../common/dailog/OperationDialog";

function PostPropertyChangeDialog(props: {
  onClose: () => void;
  post: HttpTimelinePostInfo;
  onSuccess: (post: HttpTimelinePostInfo) => void;
}): React.ReactElement | null {
  const { onClose, post, onSuccess } = props;

  return (
    <OperationDialog
      title="timeline.changePostPropertyDialog.title"
      onClose={onClose}
      open
      inputScheme={[
        {
          label: "timeline.changePostPropertyDialog.time",
          type: "datetime",
          initValue: post.time,
        },
      ]}
      onProcess={([time]) => {
        return getHttpTimelineClient().patchPost(post.timelineName, post.id, {
          time: time === "" ? undefined : new Date(time).toISOString(),
        });
      }}
      onSuccessAndClose={onSuccess}
    />
  );
}

export default PostPropertyChangeDialog;
