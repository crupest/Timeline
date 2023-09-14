import {
  getHttpTimelineClient,
  HttpTimelineInfo,
  HttpTimelinePatchRequest,
  kTimelineVisibilities,
  TimelineVisibility,
} from "~src/http/timeline";

import OperationDialog from "~src/components/dialog/OperationDialog";

interface TimelinePropertyChangeDialogProps {
  timeline: HttpTimelineInfo;
  onChange: () => void;
}

const labelMap: { [key in TimelineVisibility]: string } = {
  Private: "timeline.visibility.private",
  Public: "timeline.visibility.public",
  Register: "timeline.visibility.register",
};

export default function TimelinePropertyChangeDialog({
  timeline,
  onChange,
}: TimelinePropertyChangeDialogProps) {
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
      onProcess={({ title, visibility, description }) => {
        const req: HttpTimelinePatchRequest = {};
        if (title !== timeline.title) {
          req.title = title;
        }
        if (visibility !== timeline.visibility) {
          req.visibility = visibility;
        }
        if (description !== timeline.description) {
          req.description = description;
        }
        return getHttpTimelineClient()
          .patchTimeline(timeline.owner.username, timeline.nameV2, req)
          .then(onChange);
      }}
    />
  );
}

