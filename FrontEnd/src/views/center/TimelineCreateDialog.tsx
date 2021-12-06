import React from "react";
import { useNavigate } from "react-router-dom";

import { validateTimelineName } from "@/services/timeline";
import OperationDialog from "../common/dailog/OperationDialog";
import { getHttpTimelineClient, HttpTimelineInfo } from "@/http/timeline";

interface TimelineCreateDialogProps {
  open: boolean;
  close: () => void;
}

const TimelineCreateDialog: React.FC<TimelineCreateDialogProps> = (props) => {
  const navigate = useNavigate();

  return (
    <OperationDialog
      open={props.open}
      onClose={props.close}
      themeColor="success"
      title="home.createDialog.title"
      inputScheme={
        [
          {
            type: "text",
            label: "home.createDialog.name",
            helperText: "home.createDialog.nameFormat",
          },
        ] as const
      }
      inputValidator={([name]) => {
        if (name.length === 0) {
          return { 0: "home.createDialog.noEmpty" };
        } else if (name.length > 26) {
          return { 0: "home.createDialog.tooLong" };
        } else if (!validateTimelineName(name)) {
          return { 0: "home.createDialog.badFormat" };
        } else {
          return null;
        }
      }}
      onProcess={([name]): Promise<HttpTimelineInfo> =>
        getHttpTimelineClient().postTimeline({ name })
      }
      onSuccessAndClose={(timeline: HttpTimelineInfo) => {
        navigate(`timelines/${timeline.name}`);
      }}
      failurePrompt={(e) => `${e as string}`}
    />
  );
};

export default TimelineCreateDialog;
