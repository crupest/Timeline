import React from "react";
import classnames from "classnames";
import { Link } from "react-router-dom";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import { pushAlert } from "@/services/alert";

import useClickOutside from "@/utilities/useClickOutside";

import UserAvatar from "../common/user/UserAvatar";
import Card from "../common/Card";
import FlatButton from "../common/button/FlatButton";
import ConfirmDialog from "../common/dailog/ConfirmDialog";
import TimelineLine from "./TimelineLine";
import TimelinePostContentView from "./TimelinePostContentView";
import PostPropertyChangeDialog from "./PostPropertyChangeDialog";

export interface TimelinePostViewProps {
  post: HttpTimelinePostInfo;
  className?: string;
  style?: React.CSSProperties;
  cardStyle?: React.CSSProperties;
  onChanged: (post: HttpTimelinePostInfo) => void;
  onDeleted: () => void;
}

const TimelinePostView: React.FC<TimelinePostViewProps> = (props) => {
  const { post, className, style, cardStyle, onChanged, onDeleted } = props;

  const [operationMaskVisible, setOperationMaskVisible] =
    React.useState<boolean>(false);
  const [dialog, setDialog] = React.useState<
    "delete" | "changeproperty" | null
  >(null);

  const [maskElement, setMaskElement] = React.useState<HTMLElement | null>(
    null
  );

  useClickOutside(maskElement, () => setOperationMaskVisible(false));

  const cardRef = React.useRef<HTMLDivElement>(null);
  React.useEffect(() => {
    const cardIntersectionObserver = new IntersectionObserver(([e]) => {
      if (e.intersectionRatio > 0) {
        if (cardRef.current != null) {
          cardRef.current.style.animationName = "timeline-post-enter";
        }
      }
    });
    if (cardRef.current) {
      cardIntersectionObserver.observe(cardRef.current);
    }

    return () => {
      cardIntersectionObserver.disconnect();
    };
  }, []);

  return (
    <div
      id={`timeline-post-${post.id}`}
      className={classnames("timeline-item", className)}
      style={style}
    >
      <TimelineLine center="node" />
      <Card
        ref={cardRef}
        className="timeline-item-card enter-animation"
        style={cardStyle}
      >
        {post.editable ? (
          <i
            className="bi-chevron-down icon-button primary-enhance cru-float-right"
            onClick={(e) => {
              setOperationMaskVisible(true);
              e.stopPropagation();
            }}
          />
        ) : null}
        <div className="timeline-item-header">
          <span className="me-2">
            <span>
              <UserAvatar
                username={post.author.username}
                className="timeline-avatar me-1"
              />
              <small className="text-dark me-2">{post.author.nickname}</small>
              <small className="text-secondary white-space-no-wrap">
                {new Date(post.time).toLocaleTimeString()}
              </small>
            </span>
          </span>
        </div>
        <div className="timeline-content">
          <TimelinePostContentView post={post} />
        </div>
        {operationMaskVisible ? (
          <div
            ref={setMaskElement}
            className="timeline-post-item-options-mask"
            onClick={() => {
              setOperationMaskVisible(false);
            }}
          >
            <FlatButton
              text="changeProperty"
              onClick={(e) => {
                setDialog("changeproperty");
                e.stopPropagation();
              }}
            />
            <FlatButton
              text="delete"
              color="danger"
              onClick={(e) => {
                setDialog("delete");
                e.stopPropagation();
              }}
            />
          </div>
        ) : null}
      </Card>
      <ConfirmDialog
        title="timeline.post.deleteDialog.title"
        body="timeline.post.deleteDialog.prompt"
        open={dialog === "delete"}
        onClose={() => {
          setDialog(null);
          setOperationMaskVisible(false);
        }}
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
      />
      <PostPropertyChangeDialog
        open={dialog === "changeproperty"}
        onClose={() => {
          setDialog(null);
          setOperationMaskVisible(false);
        }}
        post={post}
        onSuccess={onChanged}
      />
    </div>
  );
};

export default TimelinePostView;
