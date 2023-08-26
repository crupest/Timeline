import { useC, Text, ThemeColor } from "../common";

import Dialog from "./Dialog";
import DialogContainer from "./DialogContainer";

export default function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  body,
  color,
  bodyColor,
}: {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: Text;
  body: Text;
  color?: ThemeColor;
  bodyColor?: ThemeColor;
}) {
  const c = useC();

  return (
    <Dialog onClose={onClose} open={open}>
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
              onClick: onClose,
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
                onClose();
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
