import { useC, Text, ThemeColor } from "../common";

import Dialog from "./Dialog";
import DialogContainer from "./DialogContainer";
import { useCloseDialog } from "./DialogProvider";

export default function ConfirmDialog({
  onConfirm,
  title,
  body,
  color,
}: {
  onConfirm: () => void;
  title: Text;
  body: Text;
  color?: ThemeColor;
  bodyColor?: ThemeColor;
}) {
  const c = useC();

  const closeDialog = useCloseDialog();

  return (
    <Dialog color={color ?? "danger"}>
      <DialogContainer
        title={title}
        titleColor={color ?? "danger"}
        buttonsV2={[
          {
            key: "cancel",
            type: "normal",
            action: "minor",

            text: "operationDialog.cancel",
            onClick: closeDialog,
          },
          {
            key: "confirm",
            type: "normal",
            action: "major",
            text: "operationDialog.confirm",
            color: "danger",
            onClick: () => {
              onConfirm();
              closeDialog();
            },
          },
        ]}
      >
        <div>{c(body)}</div>
      </DialogContainer>
    </Dialog>
  );
}
