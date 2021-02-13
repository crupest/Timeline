import React from "react";
import { useHistory } from "react-router";
import { Trans } from "react-i18next";

import { getHttpTimelineClient } from "@/http/timeline";

import OperationDialog from "../common/OperationDialog";

interface TimelineDeleteDialog {
  open: boolean;
  name: string;
  close: () => void;
}

const TimelineDeleteDialog: React.FC<TimelineDeleteDialog> = (props) => {
  const history = useHistory();

  const { name } = props;

  return (
    <OperationDialog
      open={props.open}
      close={props.close}
      title="timeline.deleteDialog.title"
      themeColor="danger"
      inputPrompt={() => {
        return (
          <Trans i18nKey="timeline.deleteDialog.inputPrompt">
            0<code className="mx-2">{{ name }}</code>2
          </Trans>
        );
      }}
      inputScheme={[
        {
          type: "text",
        },
      ]}
      inputValidator={([value]) => {
        if (value !== name) {
          return { 0: "timeline.deleteDialog.notMatch" };
        } else {
          return null;
        }
      }}
      onProcess={() => {
        return getHttpTimelineClient().deleteTimeline(name);
      }}
      onSuccessAndClose={() => {
        history.replace("/");
      }}
    />
  );
};

export default TimelineDeleteDialog;
