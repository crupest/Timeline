import * as React from "react";

import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePatchRequest,
  kTimelineVisibilities,
  TimelineVisibility,
} from "@/http/timeline";

import OperationDialog from "../common/dailog/OperationDialog";

export interface TimelinePropertyChangeDialogProps {
  open: boolean;
  close: () => void;
  timeline: HttpTimelineInfo;
  onChange: () => void;
}

const labelMap: { [key in TimelineVisibility]: string } = {
  Private: "timeline.visibility.private",
  Public: "timeline.visibility.public",
  Register: "timeline.visibility.register",
};

const TimelinePropertyChangeDialog: React.FC<
  TimelinePropertyChangeDialogProps
> = (props) => {
  const { timeline, onChange } = props;

  return (
    <OperationDialog
      title={"timeline.dialogChangeProperty.title"}
      inputScheme={
        [
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
          {
            type: "color",
            label: "timeline.dialogChangeProperty.color",
            initValue: timeline.color ?? null,
            canBeNull: true,
          },
        ] as const
      }
      open={props.open}
      onClose={props.close}
      onProcess={([newTitle, newVisibility, newDescription, newColor]) => {
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
        const nc = newColor ?? "";
        if (nc !== timeline.color) {
          req.color = nc;
        }
        return getHttpTimelineClient()
          .patchTimeline(timeline.owner.username, timeline.nameV2, req)
          .then(onChange);
      }}
    />
  );
};

export default TimelinePropertyChangeDialog;
