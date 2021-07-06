import React from "react";
import ReactDOM from "react-dom";
import { CSSTransition } from "react-transition-group";

import "./Dialog.css";

export interface DialogProps {
  onClose: () => void;
  open: boolean;
  children?: React.ReactNode;
  disableCloseOnClickOnOverlay?: boolean;
}

export default function Dialog(props: DialogProps): React.ReactElement | null {
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
        onClick={
          disableCloseOnClickOnOverlay
            ? undefined
            : () => {
                onClose();
              }
        }
      >
        <div
          className="cru-dialog-container"
          onClick={(e) => e.stopPropagation()}
        >
          {children}
        </div>
      </div>
    </CSSTransition>,
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    document.getElementById("portal")!
  );
}
