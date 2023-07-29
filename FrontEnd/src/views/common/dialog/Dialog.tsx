import { ReactNode, useRef } from "react";
import ReactDOM from "react-dom";
import { CSSTransition } from "react-transition-group";
import classNames from "classnames";

import { ThemeColor } from "../common";

import "./Dialog.css";

const optionalPortalElement = document.getElementById("portal");
if (optionalPortalElement == null) {
  throw new Error("Portal element not found");
}
const portalElement = optionalPortalElement;

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
  color = color ?? "primary";

  const nodeRef = useRef(null);

  return ReactDOM.createPortal(
    <CSSTransition
      nodeRef={nodeRef}
      mountOnEnter
      unmountOnExit
      in={open}
      timeout={300}
      classNames="cru-dialog"
    >
      <div
        ref={nodeRef}
        className={classNames("cru-dialog-overlay", `cru-${color}`)}
      >
        <div
          className="cru-dialog-background"
          onClick={
            disableCloseOnClickOnOverlay
              ? undefined
              : () => {
                  onClose();
                }
          }
        />
        <div className="cru-dialog-container">{children}</div>
      </div>
    </CSSTransition>,
    portalElement,
  );
}
