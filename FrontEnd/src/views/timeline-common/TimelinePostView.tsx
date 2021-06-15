import React from "react";
import classnames from "classnames";
import { Link } from "react-router-dom";

import { getHttpTimelineClient, HttpTimelinePostInfo } from "@/http/timeline";

import { pushAlert } from "@/services/alert";

import UserAvatar from "../common/user/UserAvatar";
import Card from "../common/Card";
import FlatButton from "../common/button/FlatButton";
import TimelineLine from "./TimelineLine";
import TimelinePostContentView from "./TimelinePostContentView";
import TimelinePostDeleteConfirmDialog from "./TimelinePostDeleteConfirmDialog";
import PostPropertyChangeDialog from "./PostPropertyChangeDialog";

export interface TimelinePostViewProps {
  post: HttpTimelinePostInfo;
  current?: boolean;
  className?: string;
  style?: React.CSSProperties;
  cardStyle?: React.CSSProperties;
  onChanged: (post: HttpTimelinePostInfo) => void;
  onDeleted: () => void;
}

const TimelinePostView: React.FC<TimelinePostViewProps> = (props) => {
  const { post, className, style, cardStyle, onChanged, onDeleted } = props;
  const current = props.current === true;

  const [operationMaskVisible, setOperationMaskVisible] =
    React.useState<boolean>(false);
  const [dialog, setDialog] = React.useState<
    "delete" | "changeproperty" | null
  >(null);

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
      className={classnames("timeline-item", current && "current", className)}
      style={style}
    >
      <TimelineLine center="node" current={current} />
      <Card ref={cardRef} className="timeline-item-card" style={cardStyle}>
        {post.editable ? (
          <i
            className="bi-chevron-down text-info icon-button float-end"
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
            className="timeline-post-item-options-mask d-flex justify-content-around align-items-center"
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
      {dialog === "delete" ? (
        <TimelinePostDeleteConfirmDialog
          onClose={() => {
            setDialog(null);
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
      ) : dialog === "changeproperty" ? (
        <PostPropertyChangeDialog
          onClose={() => {
            setDialog(null);
            setOperationMaskVisible(false);
          }}
          post={post}
          onSuccess={onChanged}
        />
      ) : null}
    </div>
  );
};

export default TimelinePostView;
