import { ReactNode, useRef } from "react";
import classNames from "classnames";
import { CSSTransition } from "react-transition-group";

import InPortal from "../InPortal";

import "./DialogOverlay.css";

interface DialogOverlayProps {
  open: boolean;
  onClose: () => void;
  transitionClassNames: string;
  overlayClassName?: string;
  containerClassName?: string;
  children?: ReactNode;
  disableCloseOnClickOnOverlay?: boolean;
}

export default function DialogOverlay({
  open,
  onClose,
  transitionClassNames,
  overlayClassName,
  containerClassName,
  children,
  disableCloseOnClickOnOverlay,
}: DialogOverlayProps) {
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
        classNames={transitionClassNames}
      >
        <div
          ref={nodeRef}
          className={classNames("cru-dialog-overlay", overlayClassName)}
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
          <div
            className={classNames("cru-dialog-container", containerClassName)}
          >
            {children}
          </div>
        </div>
      </CSSTransition>
    </InPortal>
  );
}
