import React from "react";
import clsx from "clsx";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import Svg from "react-inlinesvg";
import chevronDownIcon from "bootstrap-icons/icons/chevron-down.svg";
import trashIcon from "bootstrap-icons/icons/trash.svg";
import { Modal, Button } from "react-bootstrap";

import { useAvatar } from "@/services/user";
import { TimelinePostInfo } from "@/services/timeline";

import BlobImage from "../common/BlobImage";

const TimelinePostDeleteConfirmDialog: React.FC<{
  toggle: () => void;
  onConfirm: () => void;
}> = ({ toggle, onConfirm }) => {
  const { t } = useTranslation();

  return (
    <Modal toggle={toggle} isOpen centered>
      <Modal.Header>
        <Modal.Title className="text-danger">
          {t("timeline.post.deleteDialog.title")}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>{t("timeline.post.deleteDialog.prompt")}</Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={toggle}>
          {t("operationDialog.cancel")}
        </Button>
        <Button
          variant="danger"
          onClick={() => {
            onConfirm();
            toggle();
          }}
        >
          {t("operationDialog.confirm")}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

export interface TimelineItemProps {
  post: TimelinePostInfo;
  current?: boolean;
  more?: {
    isOpen: boolean;
    toggle: () => void;
    onDelete: () => void;
  };
  onClick?: () => void;
  onResize?: () => void;
  className?: string;
  style?: React.CSSProperties;
}

const TimelineItem: React.FC<TimelineItemProps> = (props) => {
  const { i18n } = useTranslation();

  const current = props.current === true;

  const { more, onResize } = props;

  const avatar = useAvatar(props.post.author.username);

  const [deleteDialog, setDeleteDialog] = React.useState<boolean>(false);
  const toggleDeleteDialog = React.useCallback(
    () => setDeleteDialog((old) => !old),
    []
  );

  return (
    <div
      className={clsx(
        "timeline-item position-relative",
        current && "current",
        props.className
      )}
      onClick={props.onClick}
      style={props.style}
    >
      <div className="timeline-line-area-container">
        <div className="timeline-line-area">
          <div className="timeline-line-segment start"></div>
          <div className="timeline-line-node-container">
            <div className="timeline-line-node"></div>
          </div>
          <div className="timeline-line-segment end"></div>
          {current && <div className="timeline-line-segment current-end" />}
        </div>
      </div>
      <div className="timeline-content-area">
        <div>
          <span className="mr-2">
            <span className="text-primary white-space-no-wrap mr-2">
              {props.post.time.toLocaleString(i18n.languages)}
            </span>
            <small className="text-dark">{props.post.author.nickname}</small>
          </span>
          {more != null ? (
            <Svg
              src={chevronDownIcon}
              className="text-info icon-button"
              onClick={(e) => {
                more.toggle();
                e.stopPropagation();
              }}
            />
          ) : null}
        </div>
        <div className="timeline-content">
          <Link
            className="float-left m-2"
            to={"/users/" + props.post.author.username}
          >
            <BlobImage
              onLoad={onResize}
              blob={avatar}
              className="avatar rounded"
            />
          </Link>
          {(() => {
            const { content } = props.post;
            if (content.type === "text") {
              return content.text;
            } else {
              return (
                <BlobImage
                  onLoad={onResize}
                  blob={content.data}
                  className="timeline-content-image"
                />
              );
            }
          })()}
        </div>
      </div>
      {more != null && more.isOpen ? (
        <>
          <div
            className="position-absolute position-lt w-100 h-100 mask d-flex justify-content-center align-items-center"
            onClick={more.toggle}
          >
            <Svg
              src={trashIcon}
              className="text-danger icon-button large"
              onClick={(e) => {
                toggleDeleteDialog();
                e.stopPropagation();
              }}
            />
          </div>
          {deleteDialog ? (
            <TimelinePostDeleteConfirmDialog
              toggle={() => {
                toggleDeleteDialog();
                more.toggle();
              }}
              onConfirm={more.onDelete}
            />
          ) : null}
        </>
      ) : null}
    </div>
  );
};

export default TimelineItem;
