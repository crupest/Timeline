import React from "react";
import { useHistory } from "react-router";

import { validateTimelineName, timelineService } from "../data/timeline";
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
      inputScheme={[
        {
          type: "text",
          label: "home.createDialog.name",
          helperText: "home.createDialog.nameFormat",
          validator: (name) => {
            if (name.length === 0) {
              return "home.createDialog.noEmpty";
            } else if (name.length > 26) {
              return "home.createDialog.tooLong";
            } else if (!validateTimelineName(name)) {
              return "home.createDialog.badFormat";
            } else {
              return null;
            }
          },
        },
      ]}
      onProcess={([name]) => {
        nameSaved = name as string;
        return timelineService.createTimeline(nameSaved).toPromise();
      }}
      onSuccessAndClose={() => {
        history.push(`timelines/${nameSaved}`);
      }}
      failurePrompt={(e) => `${e as string}`}
    />
  );
};

export default TimelineCreateDialog;
