import React from "react";
import { useTranslation } from "react-i18next";
import { Dropdown, Button } from "react-bootstrap";

import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";
import { useAvatar } from "@/services/user";

import BlobImage from "../common/BlobImage";
import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";
import InfoCardTemplate from "../timeline-common/InfoCardTemplate";

export type PersonalTimelineManageItem = "avatar" | "nickname";

export type UserInfoCardProps = TimelineCardComponentProps<
  PersonalTimelineManageItem
>;

const UserInfoCard: React.FC<UserInfoCardProps> = (props) => {
  const {
    timeline,
    onMember,
    onManage,
    syncStatus,
    collapse,
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
      <div>
        <BlobImage blob={avatar} className="avatar" />
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
              <Dropdown.Item onClick={() => onManage("nickname")}>
                {t("timeline.manageItem.nickname")}
              </Dropdown.Item>
              <Dropdown.Item onClick={() => onManage("avatar")}>
                {t("timeline.manageItem.avatar")}
              </Dropdown.Item>
              <Dropdown.Item onClick={() => onManage("property")}>
                {t("timeline.manageItem.property")}
              </Dropdown.Item>
              <Dropdown.Item onClick={onMember}>
                {t("timeline.manageItem.member")}
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

export default UserInfoCard;
