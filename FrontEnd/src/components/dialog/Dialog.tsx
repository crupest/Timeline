import { ReactNode, useRef } from "react";
import ReactDOM from "react-dom";
import classNames from "classnames";

import { ThemeColor } from "../common";

import { useCloseDialog } from "./DialogProvider";

import "./Dialog.css";

const optionalPortalElement = document.getElementById("portal");
if (optionalPortalElement == null) {
  throw new Error("Portal element not found");
}
const portalElement = optionalPortalElement;

interface DialogProps {
  children?: ReactNode;
  disableCloseOnClickOnOverlay?: boolean;
}

export default function Dialog({
  children,
  disableCloseOnClickOnOverlay,
}: DialogProps) {
  const closeDialog = useCloseDialog();

  const lastPointerDownIdRef = useRef<number | null>(null);

  return ReactDOM.createPortal(
    <div className={classNames("cru-dialog-overlay")}>
      <div
        className="cru-dialog-background"
        onPointerDown={(e) => {
          lastPointerDownIdRef.current = e.pointerId;
        }}
        onPointerUp={(e) => {
          if (lastPointerDownIdRef.current === e.pointerId) {
            if (!disableCloseOnClickOnOverlay) closeDialog();
          }
          lastPointerDownIdRef.current = null;
        }}
      />
      <div className="cru-dialog-container">{children}</div>
    </div>,
    portalElement,
  );
}
