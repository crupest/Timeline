import { ReactNode } from "react";

import { ThemeColor } from "../common";
import { IconButton } from "../button";
import DialogOverlay from "./DialogOverlay";

import "./FullPageDialog.css";

interface FullPageDialogProps {
  open: boolean;
  onClose: () => void;
  color?: ThemeColor;
  children?: ReactNode;
  disableCloseOnClickOnOverlay?: boolean;
}

export default function FullPageDialog({
  open,
  onClose,
  color,
  children,
  disableCloseOnClickOnOverlay,
}: FullPageDialogProps) {
  return (
    <DialogOverlay
      open={open}
      onClose={onClose}
      transitionClassNames="cru-dialog-full-page"
      overlayClassName={`cru-dialog-full-page cru-theme-${color ?? "primary"}`}
      disableCloseOnClickOnOverlay={disableCloseOnClickOnOverlay}
    >
      <div className="cru-dialog-full-page-content">
        <IconButton
          icon="x-lg"
          color={color ?? "primary"}
          className="cru-dialog-full-page-back"
          onClick={onClose}
        />
        {children}
      </div>
    </DialogOverlay>
  );
}
