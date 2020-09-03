import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";
import { Dropdown, Button } from "react-bootstrap";

import { useAvatar } from "@/services/user";
import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";

import BlobImage from "../common/BlobImage";
import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";
import CollapseButton from "../timeline-common/CollapseButton";
import SyncStatusBadge from "../timeline-common/SyncStatusBadge";

export type OrdinaryTimelineManageItem = "delete";

export type TimelineInfoCardProps = TimelineCardComponentProps<
  OrdinaryTimelineManageItem
>;

const TimelineInfoCard: React.FC<TimelineInfoCardProps> = (props) => {
  const {
    timeline,
    onMember,
    onManage,
    collapse,
    syncStatus,
    toggleCollapse,
  } = props;

  const { t } = useTranslation();

  const avatar = useAvatar(timeline?.owner?.username);

  return (
    <div className={clsx("cru-card p-2 clearfix", props.className)}>
      <div className="float-right d-flex align-items-center">
        <SyncStatusBadge status={syncStatus} className="mr-2" />
        <CollapseButton collapse={collapse} onClick={toggleCollapse} />
      </div>

      <div style={{ display: collapse ? "none" : "block" }}>
        <h3 className="text-primary mx-3 d-inline-block align-middle">
          {timeline.name}
        </h3>
        <div className="d-inline-block align-middle">
          <BlobImage blob={avatar} className="avatar small rounded-circle" />
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
          {onManage != null ? (
            <Dropdown>
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
      </div>
    </div>
  );
};

export default TimelineInfoCard;
