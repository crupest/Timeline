import { ReactNode } from "react";
import ReactDOM from "react-dom";
import { CSSTransition } from "react-transition-group";

import "./Dialog.css";

const optionalPortalElement = document.getElementById("portal");
if (optionalPortalElement == null) {
  throw new Error("Portal element not found");
}
const portalElement = optionalPortalElement;

interface DialogProps {
  onClose: () => void;
  open: boolean;
  children?: ReactNode;
  disableCloseOnClickOnOverlay?: boolean;
}

export default function Dialog(props: DialogProps) {
  const { open, onClose, children, disableCloseOnClickOnOverlay } = props;

  return ReactDOM.createPortal(
    <CSSTransition
      mountOnEnter
      unmountOnExit
      in={open}
      timeout={300}
      classNames="cru-dialog"
    >
      <div
        className="cru-dialog-overlay"
        onPointerDown={
          disableCloseOnClickOnOverlay
            ? undefined
            : () => {
                onClose();
              }
        }
      >
        <div
          className="cru-dialog-container"
          onPointerDown={(e) => e.stopPropagation()}
        >
          {children}
        </div>
      </div>
    </CSSTransition>,
    portalElement,
  );
}
