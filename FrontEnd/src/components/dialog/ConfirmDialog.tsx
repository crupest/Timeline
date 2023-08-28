import { useC, Text, ThemeColor } from "../common";

import Dialog from "./Dialog";
import DialogContainer from "./DialogContainer";
import { useCloseDialog } from "./DialogProvider";

export default function ConfirmDialog({
  onConfirm,
  title,
  body,
  color,
  bodyColor,
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
    <Dialog>
      <DialogContainer
        title={title}
        titleColor={color ?? "danger"}
        buttons={[
          {
            key: "cancel",
            type: "normal",
            props: {
              text: "operationDialog.cancel",
              color: "secondary",
              outline: true,
              onClick: closeDialog,
            },
          },
          {
            key: "confirm",
            type: "normal",
            props: {
              text: "operationDialog.confirm",
              color: "danger",
              onClick: () => {
                onConfirm();
                closeDialog();
              },
            },
          },
        ]}
      >
        <div className={`cru-${bodyColor ?? "primary"}`}>{c(body)}</div>
      </DialogContainer>
    </Dialog>
  );
}
