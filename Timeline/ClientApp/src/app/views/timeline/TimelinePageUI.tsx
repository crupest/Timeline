import React from "react";

import TimelinePageTemplateUI, {
  TimelinePageTemplateUIProps,
} from "../timeline-common/TimelinePageTemplateUI";

import TimelineInfoCard, {
  OrdinaryTimelineManageItem,
} from "./TimelineInfoCard";

export type TimelinePageUIProps = Omit<
  TimelinePageTemplateUIProps<OrdinaryTimelineManageItem>,
  "CardComponent"
>;

const TimelinePageUI: React.FC<TimelinePageUIProps> = (props) => {
  return <TimelinePageTemplateUI {...props} CardComponent={TimelineInfoCard} />;
};

export default TimelinePageUI;
