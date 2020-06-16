import React from 'react';
import clsx from 'clsx';
import {
  Row,
  Col,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from 'reactstrap';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Svg from 'react-inlinesvg';

import chevronDownIcon from 'bootstrap-icons/icons/chevron-down.svg';
import trashIcon from 'bootstrap-icons/icons/trash.svg';

import { TimelinePostInfo } from '../data/timeline';
import { useAvatarUrlWithGivenVersion } from '../user/api';

const TimelinePostDeleteConfirmDialog: React.FC<{
  toggle: () => void;
  onConfirm: () => void;
}> = ({ toggle, onConfirm }) => {
  const { t } = useTranslation();

  return (
    <Modal toggle={toggle} isOpen centered>
      <ModalHeader className="text-danger">
        {t('timeline.post.deleteDialog.title')}
      </ModalHeader>
      <ModalBody>{t('timeline.post.deleteDialog.prompt')}</ModalBody>
      <ModalFooter>
        <Button color="secondary" onClick={toggle}>
          {t('operationDialog.cancel')}
        </Button>
        <Button
          color="danger"
          onClick={() => {
            onConfirm();
            toggle();
          }}
        >
          {t('operationDialog.confirm')}
        </Button>
      </ModalFooter>
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
  avatarVersion?: number;
  onResize?: () => void;
  className?: string;
  style?: React.CSSProperties;
}

const TimelineItem: React.FC<TimelineItemProps> = (props) => {
  const { i18n } = useTranslation();

  const current = props.current === true;

  const { more, onResize } = props;

  const avatarUrl = useAvatarUrlWithGivenVersion(
    props.avatarVersion,
    props.post.author._links.avatar
  );

  const [deleteDialog, setDeleteDialog] = React.useState<boolean>(false);
  const toggleDeleteDialog = React.useCallback(
    () => setDeleteDialog((old) => !old),
    []
  );

  return (
    <Row
      className={clsx(
        'position-relative flex-nowrap',
        current && 'current',
        props.className
      )}
      onClick={props.onClick}
      style={props.style}
    >
      <Col className="timeline-line-area">
        <div className="timeline-line-segment start"></div>
        <div className="timeline-line-node-container">
          <div className="timeline-line-node"></div>
        </div>
        <div className="timeline-line-segment end"></div>
        {current && <div className="timeline-line-segment current-end" />}
      </Col>
      <Col className="timeline-pt-start">
        <Row className="flex-nowrap">
          <div className="col-auto flex-shrink-1 px-0">
            <Row className="ml-n3 mr-0 align-items-center">
              <span className="ml-3 text-primary white-space-no-wrap">
                {props.post.time.toLocaleString(i18n.languages)}
              </span>
              <small className="text-dark ml-3">
                {props.post.author.nickname}
              </small>
            </Row>
          </div>
          {more != null ? (
            <div className="col-auto px-2 d-flex justify-content-center align-items-center">
              <Svg
                src={chevronDownIcon}
                className="text-info icon-button"
                onClick={(e: Event) => {
                  more.toggle();
                  e.stopPropagation();
                }}
              />
            </div>
          ) : null}
        </Row>
        <p className="row d-block mb-0 timeline-content clearfix">
          <Link
            className="float-right float-sm-left mx-2"
            to={'/users/' + props.post.author.username}
          >
            <img onLoad={onResize} src={avatarUrl} className="avatar rounded" />
          </Link>
          {(() => {
            const { content } = props.post;
            if (content.type === 'text') {
              return content.text;
            } else {
              return (
                <img
                  onLoad={onResize}
                  src={content.url}
                  className="timeline-content-image"
                />
              );
            }
          })()}
        </p>
      </Col>
      {more != null && more.isOpen ? (
        <>
          <div
            className="position-absolute position-lt w-100 h-100 mask d-flex justify-content-center align-items-center"
            onClick={more.toggle}
          >
            <Svg
              src={trashIcon}
              className="text-danger large-icon-button"
              onClick={(e: Event) => {
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
    </Row>
  );
};

export default TimelineItem;
