import React from "react";
import { useTranslation } from "react-i18next";
import { Dropdown, Button } from "react-bootstrap";
import Svg from "react-inlinesvg";
import bookmarkIcon from "bootstrap-icons/icons/bookmark.svg";

import { useAvatar } from "@/services/user";
import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";

import BlobImage from "../common/BlobImage";
import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";
import InfoCardTemplate from "../timeline-common/InfoCardTemplate";

export type OrdinaryTimelineManageItem = "delete";

export type TimelineInfoCardProps = TimelineCardComponentProps<OrdinaryTimelineManageItem>;

const TimelineInfoCard: React.FC<TimelineInfoCardProps> = (props) => {
  const {
    timeline,
    collapse,
    onMember,
    onBookmark,
    onManage,
    syncStatus,
    toggleCollapse,
  } = props;

  const { t } = useTranslation();

  const avatar = useAvatar(timeline?.owner?.username);

  return (
    <InfoCardTemplate
      className={props.className}
      syncStatus={syncStatus}
      collapse={collapse}
      toggleCollapse={toggleCollapse}
    >
      <h3 className="text-primary d-inline-block align-middle">
        {timeline.title}
        <small className="ml-3 text-secondary">{timeline.name}</small>
      </h3>
      <div className="align-middle">
        <BlobImage blob={avatar} className="avatar small rounded-circle mr-3" />
        {timeline.owner.nickname}
        <small className="ml-3 text-secondary">
          @{timeline.owner.username}
        </small>
      </div>
      <p className="mb-0">{timeline.description}</p>
      <small className="mt-1 d-block">
        {t(timelineVisibilityTooltipTranslationMap[timeline.visibility])}
      </small>
      <div className="text-right mt-2">
        {onBookmark != null ? (
          <Svg
            src={bookmarkIcon}
            className="icon-button text-yellow mr-3"
            onClick={onBookmark}
          />
        ) : null}
        {onManage != null ? (
          <Dropdown className="d-inline-block">
            <Dropdown.Toggle variant="outline-primary">
              {t("timeline.manage")}
            </Dropdown.Toggle>
            <Dropdown.Menu>
              <Dropdown.Item onClick={() => onManage("property")}>
                {t("timeline.manageItem.property")}
              </Dropdown.Item>
              <Dropdown.Item onClick={onMember}>
                {t("timeline.manageItem.member")}
              </Dropdown.Item>
              <Dropdown.Divider />
              <Dropdown.Item
                className="text-danger"
                onClick={() => onManage("delete")}
              >
                {t("timeline.manageItem.delete")}
              </Dropdown.Item>
            </Dropdown.Menu>
          </Dropdown>
        ) : (
          <Button variant="outline-primary" onClick={onMember}>
            {t("timeline.memberButton")}
          </Button>
        )}
      </div>
    </InfoCardTemplate>
  );
};

export default TimelineInfoCard;
