import * as React from "react";
import { useNavigate } from "react-router-dom";

import { validateTimelineName } from "~src/services/timeline";
import { getHttpTimelineClient, HttpTimelineInfo } from "~src/http/timeline";

import OperationDialog from "../common/dialog/OperationDialog";
import { useUserLoggedIn } from "~src/services/user";

interface TimelineCreateDialogProps {
  open: boolean;
  close: () => void;
}

const TimelineCreateDialog: React.FC<TimelineCreateDialogProps> = (props) => {
  const navigate = useNavigate();

  const user = useUserLoggedIn();

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
        navigate(`${user.username}/${timeline.nameV2}`);
      }}
      failurePrompt={(e) => `${e as string}`}
    />
  );
};

export default TimelineCreateDialog;
