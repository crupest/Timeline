import { ReactNode } from "react";
import { createPortal } from "react-dom";
import classNames from "classnames";

import { ThemeColor, UiLogicError } from "../common";
import { IconButton } from "../button";

import { useCloseDialog } from "./DialogProvider";

import "./FullPageDialog.css";

const optionalPortalElement = document.getElementById("portal");
if (optionalPortalElement == null) {
  throw new UiLogicError();
}
const portalElement = optionalPortalElement;

interface FullPageDialogProps {
  color?: ThemeColor;
  contentContainerClassName?: string;
  children: ReactNode;
}

export default function FullPageDialog({
  color,
  children,
  contentContainerClassName,
}: FullPageDialogProps) {
  const closeDialog = useCloseDialog();

  return createPortal(
    <div className={`cru-dialog-full-page cru-theme-${color ?? "primary"}`}>
      <div className="cru-dialog-full-page-top-bar">
        <IconButton
          icon="arrow-left"
          className="cru-dialog-full-page-back-button"
          onClick={closeDialog}
        />
      </div>
      <div
        className={classNames(
          "cru-dialog-full-page-content-container",
          contentContainerClassName,
        )}
      >
        {children}
      </div>
    </div>,
    portalElement,
  );
}
