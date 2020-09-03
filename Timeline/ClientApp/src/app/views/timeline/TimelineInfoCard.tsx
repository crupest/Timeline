import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";
import { Dropdown, Button } from "react-bootstrap";

import { useAvatar } from "@/services/user";
import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";

import BlobImage from "../common/BlobImage";
import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";

export type OrdinaryTimelineManageItem = "delete";

export type TimelineInfoCardProps = TimelineCardComponentProps<
  OrdinaryTimelineManageItem
>;

const TimelineInfoCard: React.FC<TimelineInfoCardProps> = (props) => {
  const { onMember, onManage } = props;

  const { t } = useTranslation();

  const avatar = useAvatar(props.timeline.owner.username);

  return (
    <div className={clsx("rounded border p-2 bg-light", props.className)}>
      <h3 className="text-primary mx-3 d-inline-block align-middle">
        {props.timeline.name}
      </h3>
      <div className="d-inline-block align-middle">
        <BlobImage blob={avatar} className="avatar small rounded-circle" />
        {props.timeline.owner.nickname}
        <small className="ml-3 text-secondary">
          @{props.timeline.owner.username}
        </small>
      </div>
      <p className="mb-0">{props.timeline.description}</p>
      <small className="mt-1 d-block">
        {t(timelineVisibilityTooltipTranslationMap[props.timeline.visibility])}
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
  );
};

export default TimelineInfoCard;
