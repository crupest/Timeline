import React from "react";

import TimelinePageTemplateUI, {
  TimelinePageTemplateUIProps,
} from "../timeline-common/TimelinePageTemplateUI";

import UserInfoCard, { PersonalTimelineManageItem } from "./UserInfoCard";

export type UserPageUIProps = Omit<
  TimelinePageTemplateUIProps<PersonalTimelineManageItem>,
  "CardComponent"
>;

const UserPageUI: React.FC<UserPageUIProps> = (props) => {
  return <TimelinePageTemplateUI {...props} CardComponent={UserInfoCard} />;
};

export default UserPageUI;
