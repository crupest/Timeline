import React from "react";

import { useAvatar } from "@/services/user";

import BlobImage from "../common/BlobImage";
import TimelineCardTemplate, {
  TimelineCardTemplateProps,
} from "../timeline-common/TimelineCardTemplate";
import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";

export type OrdinaryTimelineManageItem = "delete";

export type TimelineInfoCardProps = TimelineCardComponentProps<OrdinaryTimelineManageItem>;

const TimelineInfoCard: React.FC<TimelineInfoCardProps> = (props) => {
  const { timeline, operations } = props;
  const { onManage, onMember } = operations;

  const avatar = useAvatar(timeline?.owner?.username);

  return (
    <TimelineCardTemplate
      infoArea={
        <>
          <h3 className="text-primary d-inline-block align-middle">
            {timeline.title}
            <small className="ml-3 text-secondary">{timeline.name}</small>
          </h3>
          <div className="align-middle">
            <BlobImage
              blob={avatar}
              className="avatar small rounded-circle mr-3"
            />
            {timeline.owner.nickname}
            <small className="ml-3 text-secondary">
              @{timeline.owner.username}
            </small>
          </div>
        </>
      }
      manageArea={((): TimelineCardTemplateProps["manageArea"] => {
        if (onManage == null) {
          return { type: "member", onMember };
        } else {
          return {
            type: "manage",
            items: [
              {
                type: "button",
                text: "timeline.manageItem.property",
                onClick: () => onManage("property"),
              },
              {
                type: "button",
                onClick: onMember,
                text: "timeline.manageItem.member",
              },
              { type: "divider" },
              {
                type: "button",
                onClick: () => onManage("delete"),
                color: "danger",
                text: "timeline.manageItem.delete",
              },
            ],
          };
        }
      })()}
      {...props}
    />
  );
};

export default TimelineInfoCard;
