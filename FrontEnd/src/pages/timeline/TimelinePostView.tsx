import { useState } from "react";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "~src/http/timeline";

import { pushAlert } from "~src/services/alert";

import { useClickOutside } from "~src/utilities/hooks";

import UserAvatar from "~src/components/user/UserAvatar";
import { useDialog } from "~src/components/dialog";
import FlatButton from "~src/components/button/FlatButton";
import ConfirmDialog from "~src/components/dialog/ConfirmDialog";
import TimelinePostContentView from "./TimelinePostContentView";
import IconButton from "~src/components/button/IconButton";

import TimelinePostContainer from "./TimelinePostContainer";
import TimelinePostCard from "./TimelinePostCard";

import "./TimelinePostView.css";

interface TimelinePostViewProps {
  post: HttpTimelinePostInfo;
  className?: string;
  onChanged: (post: HttpTimelinePostInfo) => void;
  onDeleted: () => void;
}

export default function TimelinePostView(props: TimelinePostViewProps) {
  const { post, onDeleted } = props;

  const [operationMaskVisible, setOperationMaskVisible] =
    useState<boolean>(false);

  const { switchDialog, dialogPropsMap } = useDialog(["delete"], {
    onClose: {
      delete: () => {
        setOperationMaskVisible(false);
      },
    },
  });

  const [maskElement, setMaskElement] = useState<HTMLElement | null>(null);
  useClickOutside(maskElement, () => setOperationMaskVisible(false));

  return (
    <TimelinePostContainer>
      <TimelinePostCard className="cru-primary">
        {post.editable && (
          <IconButton
            color="primary"
            icon="chevron-down"
            className="timeline-post-edit-button"
            onClick={(e) => {
              setOperationMaskVisible(true);
              e.stopPropagation();
            }}
          />
        )}
        <div className="timeline-post-header">
          <UserAvatar
            username={post.author.username}
            className="timeline-post-author-avatar"
          />
          <small className="timeline-post-author-nickname">
            {post.author.nickname}
          </small>
          <small className="timeline-post-time">
            {new Date(post.time).toLocaleTimeString()}
          </small>
        </div>
        <div className="timeline-post-content">
          <TimelinePostContentView post={post} />
        </div>
        {operationMaskVisible ? (
          <div
            ref={setMaskElement}
            className="timeline-post-options-mask"
            onClick={() => {
              setOperationMaskVisible(false);
            }}
          >
            <FlatButton
              text="changeProperty"
              onClick={(e) => {
                e.stopPropagation();
              }}
            />
            <FlatButton
              text="delete"
              color="danger"
              onClick={(e) => {
                switchDialog("delete");
                e.stopPropagation();
              }}
            />
          </div>
        ) : null}
      </TimelinePostCard>
      <ConfirmDialog
        title="timeline.post.deleteDialog.title"
        body="timeline.post.deleteDialog.prompt"
        onConfirm={() => {
          void getHttpTimelineClient()
            .deletePost(post.timelineOwnerV2, post.timelineNameV2, post.id)
            .then(onDeleted, () => {
              pushAlert({
                type: "danger",
                message: "timeline.deletePostFailed",
              });
            });
        }}
        {...dialogPropsMap.delete}
      />
    </TimelinePostContainer>
  );
}
