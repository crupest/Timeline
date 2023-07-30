import * as React from "react";

import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePatchRequest,
  kTimelineVisibilities,
  TimelineVisibility,
} from "@/http/timeline";

import OperationDialog from "@/views/common/dialog/OperationDialog";

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
      inputs={{
        scheme: {
          inputs: [
            {
              key: "title",
              type: "text",
              label: "timeline.dialogChangeProperty.titleField",
            },
            {
              key: "visibility",
              type: "select",
              label: "timeline.dialogChangeProperty.visibility",
              options: kTimelineVisibilities.map((v) => ({
                label: labelMap[v],
                value: v,
              })),
            },
            {
              key: "description",
              type: "text",
              label: "timeline.dialogChangeProperty.description",
            },
          ],
        },
        dataInit: {
          values: {
            title: timeline.title,
            visibility: timeline.visibility,
            description: timeline.description,
          },
        },
      }}
      open={props.open}
      onClose={props.close}
      onProcess={({ title, visibility, description }) => {
        const req: HttpTimelinePatchRequest = {};
        if (title !== timeline.title) {
          req.title = title as string;
        }
        if (visibility !== timeline.visibility) {
          req.visibility = visibility as TimelineVisibility;
        }
        if (description !== timeline.description) {
          req.description = description as string;
        }
        return getHttpTimelineClient()
          .patchTimeline(timeline.owner.username, timeline.nameV2, req)
          .then(onChange);
      }}
    />
  );
};

export default TimelinePropertyChangeDialog;
