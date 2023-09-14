import { useNavigate } from "react-router-dom";
import { Trans } from "react-i18next";

import { getHttpTimelineClient, HttpTimelineInfo } from "~src/http/timeline";

import { OperationDialog } from "~src/components/dialog";

interface TimelineDeleteDialog {
  timeline: HttpTimelineInfo;
}

export default function TimelineDeleteDialog({ timeline }: TimelineDeleteDialog) {
  const navigate = useNavigate();

  return (
    <OperationDialog
      title="timeline.deleteDialog.title"
      color="danger"
      inputPromptNode={
        <Trans
          i18nKey="timeline.deleteDialog.inputPrompt"
          values={{ name: timeline.nameV2 }}
        >
          0<code>1</code>2
        </Trans>
      }
      inputs={{
        inputs: [
          {
            key: "name",
            type: "text",
            label: "",
          },
        ],
        validator: ({ name }, errors) => {
          if (name !== timeline.nameV2) {
            errors.name = "timeline.deleteDialog.notMatch";
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

TimelineDeleteDialog;
