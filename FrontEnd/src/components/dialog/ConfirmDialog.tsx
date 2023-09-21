import { useC, I18nText, ThemeColor } from "../common";

import Dialog from "./Dialog";
import DialogContainer from "./DialogContainer";

export default function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  body,
  color,
}: {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: I18nText;
  body: I18nText;
  color?: ThemeColor;
  bodyColor?: ThemeColor;
}) {
  const c = useC();

  return (
    <Dialog open={open} onClose={onClose} color={color ?? "danger"}>
      <DialogContainer
        title={title}
        titleColor={color ?? "danger"}
        buttonsV2={[
          {
            key: "cancel",
            type: "normal",
            action: "minor",

            text: "operationDialog.cancel",
            onClick: onClose,
          },
          {
            key: "confirm",
            type: "normal",
            action: "major",
            text: "operationDialog.confirm",
            color: "danger",
            onClick: () => {
              onConfirm();
              onClose();
            },
          },
        ]}
      >
        <div>{c(body)}</div>
      </DialogContainer>
    </Dialog>
  );
}
