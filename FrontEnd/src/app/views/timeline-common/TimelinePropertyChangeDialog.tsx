import React from "react";

import {
  TimelineVisibility,
  kTimelineVisibilities,
  TimelineChangePropertyRequest,
} from "@/services/timeline";

import OperationDialog from "../common/OperationDialog";

export interface TimelinePropertyInfo {
  title: string;
  visibility: TimelineVisibility;
  description: string;
}

export interface TimelinePropertyChangeDialogProps {
  open: boolean;
  close: () => void;
  oldInfo: TimelinePropertyInfo;
  onProcess: (request: TimelineChangePropertyRequest) => Promise<void>;
}

const labelMap: { [key in TimelineVisibility]: string } = {
  Private: "timeline.visibility.private",
  Public: "timeline.visibility.public",
  Register: "timeline.visibility.register",
};

const TimelinePropertyChangeDialog: React.FC<TimelinePropertyChangeDialogProps> = (
  props
) => {
  return (
    <OperationDialog
      title={"timeline.dialogChangeProperty.title"}
      titleColor="default"
      inputScheme={[
        {
          type: "text",
          label: "timeline.dialogChangeProperty.titleField",
          initValue: props.oldInfo.title,
        },
        {
          type: "select",
          label: "timeline.dialogChangeProperty.visibility",
          options: kTimelineVisibilities.map((v) => ({
            label: labelMap[v],
            value: v,
          })),
          initValue: props.oldInfo.visibility,
        },
        {
          type: "text",
          label: "timeline.dialogChangeProperty.description",
          initValue: props.oldInfo.description,
        },
      ]}
      open={props.open}
      close={props.close}
      onProcess={([newTitle, newVisibility, newDescription]) => {
        const req: TimelineChangePropertyRequest = {};
        if (newTitle !== props.oldInfo.title) {
          req.title = newTitle;
        }
        if (newVisibility !== props.oldInfo.visibility) {
          req.visibility = newVisibility as TimelineVisibility;
        }
        if (newDescription !== props.oldInfo.description) {
          req.description = newDescription;
        }
        return props.onProcess(req);
      }}
    />
  );
};

export default TimelinePropertyChangeDialog;
