import React from "react";

import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePatchRequest,
  kTimelineVisibilities,
  TimelineVisibility,
} from "@/http/timeline";

import OperationDialog from "../common/OperationDialog";

// TODO: Trigger resync.

export interface TimelinePropertyChangeDialogProps {
  open: boolean;
  close: () => void;
  timeline: HttpTimelineInfo;
}

const labelMap: { [key in TimelineVisibility]: string } = {
  Private: "timeline.visibility.private",
  Public: "timeline.visibility.public",
  Register: "timeline.visibility.register",
};

const TimelinePropertyChangeDialog: React.FC<TimelinePropertyChangeDialogProps> = (
  props
) => {
  const { timeline } = props;

  return (
    <OperationDialog
      title={"timeline.dialogChangeProperty.title"}
      inputScheme={[
        {
          type: "text",
          label: "timeline.dialogChangeProperty.titleField",
          initValue: timeline.title,
        },
        {
          type: "select",
          label: "timeline.dialogChangeProperty.visibility",
          options: kTimelineVisibilities.map((v) => ({
            label: labelMap[v],
            value: v,
          })),
          initValue: timeline.visibility,
        },
        {
          type: "text",
          label: "timeline.dialogChangeProperty.description",
          initValue: timeline.description,
        },
      ]}
      open={props.open}
      close={props.close}
      onProcess={([newTitle, newVisibility, newDescription]) => {
        const req: HttpTimelinePatchRequest = {};
        if (newTitle !== timeline.title) {
          req.title = newTitle;
        }
        if (newVisibility !== timeline.visibility) {
          req.visibility = newVisibility as TimelineVisibility;
        }
        if (newDescription !== timeline.description) {
          req.description = newDescription;
        }
        return getHttpTimelineClient().patchTimeline(timeline.name, req);
      }}
    />
  );
};

export default TimelinePropertyChangeDialog;
