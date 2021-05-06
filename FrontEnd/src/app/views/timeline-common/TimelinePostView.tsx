import React from "react";
import classnames from "classnames";
import { Link } from "react-router-dom";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import { pushAlert } from "@/services/alert";

import UserAvatar from "../common/user/UserAvatar";
import TimelineLine from "./TimelineLine";
import TimelinePostContentView from "./TimelinePostContentView";
import TimelinePostDeleteConfirmDialog from "./TimelinePostDeleteConfirmDialog";

export interface TimelinePostViewProps {
  post: HttpTimelinePostInfo;
  current?: boolean;
  className?: string;
  style?: React.CSSProperties;
  cardStyle?: React.CSSProperties;
  onDeleted?: () => void;
}

const TimelinePostView: React.FC<TimelinePostViewProps> = (props) => {
  const { post, className, style, cardStyle, onDeleted } = props;
  const current = props.current === true;

  const [
    operationMaskVisible,
    setOperationMaskVisible,
  ] = React.useState<boolean>(false);
  const [deleteDialog, setDeleteDialog] = React.useState<boolean>(false);

  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  const cardRef = React.useRef<HTMLDivElement>(null!);
  React.useEffect(() => {
    const cardIntersectionObserver = new IntersectionObserver(([e]) => {
      if (e.intersectionRatio > 0) {
        cardRef.current.style.animationName = "timeline-post-enter";
      }
    });
    cardIntersectionObserver.observe(cardRef.current);

    return () => {
      cardIntersectionObserver.disconnect();
    };
  }, []);

  return (
    <div
      id={`timeline-post-${post.id}`}
      className={classnames("timeline-item", current && "current", className)}
      style={style}
    >
      <TimelineLine center="node" current={current} />
      <div ref={cardRef} className="timeline-item-card" style={cardStyle}>
        {post.editable ? (
          <i
            className="bi-chevron-down text-info icon-button float-right"
            onClick={(e) => {
              setOperationMaskVisible(true);
              e.stopPropagation();
            }}
          />
        ) : null}
        <div className="timeline-item-header">
          <span className="me-2">
            <span>
              <Link to={"/users/" + props.post.author.username}>
                <UserAvatar
                  username={post.author.username}
                  className="timeline-avatar me-1"
                />
              </Link>
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
            className="position-absolute position-lt w-100 h-100 mask d-flex justify-content-center align-items-center"
            onClick={() => {
              setOperationMaskVisible(false);
            }}
          >
            <i
              className="bi-trash text-danger icon-button large"
              onClick={(e) => {
                setDeleteDialog(true);
                e.stopPropagation();
              }}
            />
          </div>
        ) : null}
      </div>
      {deleteDialog ? (
        <TimelinePostDeleteConfirmDialog
          onClose={() => {
            setDeleteDialog(false);
            setOperationMaskVisible(false);
          }}
          onConfirm={() => {
            void getHttpTimelineClient()
              .deletePost(post.timelineName, post.id)
              .then(onDeleted, () => {
                pushAlert({
                  type: "danger",
                  message: "timeline.deletePostFailed",
                });
              });
          }}
        />
      ) : null}
    </div>
  );
};

export default TimelinePostView;
