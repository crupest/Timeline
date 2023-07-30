import * as React from "react";
import { useNavigate } from "react-router-dom";
import { Trans } from "react-i18next";

import { getHttpTimelineClient, HttpTimelineInfo } from "@/http/timeline";

import OperationDialog from "@/views/common/dialog/OperationDialog";

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
      onClose={props.close}
      title="timeline.deleteDialog.title"
      color="danger"
      inputPrompt={() => (
        <Trans
          i18nKey="timeline.deleteDialog.inputPrompt"
          values={{ name: timeline.nameV2 }}
        >
          0<code className="mx-2">1</code>2
        </Trans>
      )}
      inputs={{
        inputs: [
          {
            key: "name",
            type: "text",
            label: "",
          },
        ],
        validator: ({ name }) => {
          if (name !== timeline.nameV2) {
            return { name: "timeline.deleteDialog.notMatch" };
          }
        },
      }}
      onProcess={() => {
        return getHttpTimelineClient().deleteTimeline(
          timeline.owner.username,
          timeline.nameV2,
        );
      }}
      onSuccessAndClose={() => {
        navigate("/", { replace: true });
      }}
    />
  );
};

export default TimelineDeleteDialog;
