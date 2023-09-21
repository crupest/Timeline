import { ReactNode, useRef } from "react";
import classNames from "classnames";
import { CSSTransition } from "react-transition-group";

import { ThemeColor } from "../common";

import InPortal from "../InPortal";

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
  const nodeRef = useRef(null);
  const lastPointerDownIdRef = useRef<number | null>(null);

  return (
    <InPortal>
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
          className={classNames(
            `cru-theme-${color ?? "primary"}`,
            "cru-dialog-overlay",
          )}
        >
          <div
            className="cru-dialog-background"
            onPointerDown={(e) => {
              lastPointerDownIdRef.current = e.pointerId;
            }}
            onPointerUp={(e) => {
              if (lastPointerDownIdRef.current === e.pointerId) {
                if (!disableCloseOnClickOnOverlay) onClose();
              }
              lastPointerDownIdRef.current = null;
            }}
          />
          <div className="cru-dialog-container">{children}</div>
        </div>
      </CSSTransition>
    </InPortal>
  );
}
