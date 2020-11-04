import React from "react";
import { useHistory } from "react-router";

import { validateTimelineName, timelineService } from "@/services/timeline";
import OperationDialog from "../common/OperationDialog";

interface TimelineCreateDialogProps {
  open: boolean;
  close: () => void;
}

const TimelineCreateDialog: React.FC<TimelineCreateDialogProps> = (props) => {
  const history = useHistory();

  let nameSaved: string;

  return (
    <OperationDialog
      open={props.open}
      close={props.close}
      titleColor="success"
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
      onProcess={([name]) => {
        return timelineService.createTimeline(name).toPromise();
      }}
      onSuccessAndClose={() => {
        history.push(`timelines/${nameSaved}`);
      }}
      failurePrompt={(e) => `${e as string}`}
    />
  );
};

export default TimelineCreateDialog;
