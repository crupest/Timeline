import * as React from "react";
import { useNavigate } from "react-router-dom";
import { Trans } from "react-i18next";

import { getHttpTimelineClient, HttpTimelineInfo } from "@/http/timeline";

import OperationDialog from "../common/dialog/OperationDialog";

interface TimelineDeleteDialog {
  timeline: HttpTimelineInfo;
  open: boolean;
  close: () => void;
}

const TimelineDeleteDialog: React.FC<TimelineDeleteDialog> = (props) => {
  const navigate = useNavigate();

  const { timeline } = props;

  return (
    <OperationDialog
      open={props.open}
      close={props.close}
      title="timeline.deleteDialog.title"
      themeColor="danger"
      inputPrompt={() => {
        return (
          <Trans
            i18nKey="timeline.deleteDialog.inputPrompt"
            values={{ name: timeline.nameV2 }}
          >
            0<code className="mx-2">1</code>2
          </Trans>
        );
      }}
      inputScheme={[
        {
          type: "text",
        },
      ]}
      inputValidator={([value]) => {
        if (value !== timeline.nameV2) {
          return { 0: "timeline.deleteDialog.notMatch" };
        } else {
          return null;
        }
      }}
      onProcess={() => {
        return getHttpTimelineClient().deleteTimeline(
          timeline.owner.username,
          timeline.nameV2
        );
      }}
      onSuccessAndClose={() => {
        navigate("/", { replace: true });
      }}
    />
  );
};

export default TimelineDeleteDialog;
