import { ReactNode } from "react";

import { ThemeColor } from "../common";

import DialogOverlay from "./DialogOverlay";

import "./Dialog.css";

interface DialogProps {
  open: boolean;
  onClose: () => void;
  color?: ThemeColor;
  children?: ReactNode;
  disableCloseOnClickOnOverlay?: boolean;
}

export default function Dialog({
  open,
  onClose,
  color,
  children,
  disableCloseOnClickOnOverlay,
}: DialogProps) {
  return (
    <DialogOverlay
      open={open}
      onClose={onClose}
      transitionClassNames="cru-dialog-normal"
      overlayClassName={`cru-dialog-normal cru-theme-${color ?? "primary"}`}
      disableCloseOnClickOnOverlay={disableCloseOnClickOnOverlay}
    >
      {children}
    </DialogOverlay>
  );
}
