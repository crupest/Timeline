import React from "react";
import { useHistory } from "react-router";
import { Trans } from "react-i18next";

import { getHttpTimelineClient, HttpTimelineInfo } from "@/http/timeline";

import OperationDialog from "../common/dailog/OperationDialog";

interface TimelineDeleteDialog {
  timeline: HttpTimelineInfo;
  open: boolean;
  close: () => void;
}

const TimelineDeleteDialog: React.FC<TimelineDeleteDialog> = (props) => {
  const history = useHistory();

  const { timeline } = props;

  return (
    <OperationDialog
      open={props.open}
      onClose={props.close}
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
        if (value !== timeline.name) {
          return { 0: "timeline.deleteDialog.notMatch" };
        } else {
          return null;
        }
      }}
      onProcess={() => {
        return getHttpTimelineClient().deleteTimeline(timeline.name);
      }}
      onSuccessAndClose={() => {
        history.replace("/");
      }}
    />
  );
};

export default TimelineDeleteDialog;
