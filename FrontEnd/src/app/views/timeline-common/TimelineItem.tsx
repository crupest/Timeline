import React from "react";
import clsx from "clsx";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";

import { useAvatar } from "@/services/user";
import { TimelinePostInfo } from "@/services/timeline";

import BlobImage from "../common/BlobImage";
import TimelineLine from "./TimelineLine";
import TimelinePostDeleteConfirmDialog from "./TimelinePostDeleteConfirmDialog";

export interface TimelineItemProps {
  post: TimelinePostInfo;
  current?: boolean;
  more?: {
    isOpen: boolean;
    toggle: () => void;
    onDelete: () => void;
  };
  onClick?: () => void;
  className?: string;
  style?: React.CSSProperties;
}

const TimelineItem: React.FC<TimelineItemProps> = (props) => {
  const { i18n } = useTranslation();

  const current = props.current === true;

  const { more } = props;

  const avatar = useAvatar(props.post.author.username);

  const [deleteDialog, setDeleteDialog] = React.useState<boolean>(false);

  return (
    <div
      className={clsx("timeline-item", current && "current", props.className)}
      onClick={props.onClick}
      style={props.style}
    >
      <TimelineLine center="node" current={current} />
      <div className="timeline-item-card">
        <div className="timeline-item-header">
          <span className="mr-2">
            <span>
              <Link to={"/users/" + props.post.author.username}>
                <BlobImage blob={avatar} className="timeline-avatar mr-1" />
              </Link>
              <small className="text-dark mr-2">
                {props.post.author.nickname}
              </small>
              <small className="text-secondary white-space-no-wrap">
                {props.post.time.toLocaleString(i18n.languages)}
              </small>
            </span>
          </span>
          {more != null ? (
            <i
              className="bi-chevron-down text-info icon-button"
              onClick={(e) => {
                more.toggle();
                e.stopPropagation();
              }}
            />
          ) : null}
        </div>
        <div className="timeline-content">
          {(() => {
            const { content } = props.post;
            if (content.type === "text") {
              return content.text;
            } else {
              return (
                <BlobImage
                  blob={content.data}
                  className="timeline-content-image"
                />
              );
            }
          })()}
        </div>
        {more != null && more.isOpen ? (
          <div
            className="position-absolute position-lt w-100 h-100 mask d-flex justify-content-center align-items-center"
            onClick={more.toggle}
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
      {deleteDialog && more != null ? (
        <TimelinePostDeleteConfirmDialog
          onClose={() => {
            setDeleteDialog(false);
            more.toggle();
          }}
          onConfirm={more.onDelete}
        />
      ) : null}
    </div>
  );
};

export default TimelineItem;
