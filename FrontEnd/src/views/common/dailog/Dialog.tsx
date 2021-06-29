import React from "react";
import ReactDOM from "react-dom";

import "./Dialog.css";

export interface DialogProps {
  onClose: () => void;
  open?: boolean;
  children?: React.ReactNode;
  disableCloseOnClickOnOverlay?: boolean;
}

export default function Dialog(props: DialogProps): React.ReactElement | null {
  const { open, onClose, children, disableCloseOnClickOnOverlay } = props;

  return open
    ? ReactDOM.createPortal(
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
        </div>,
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        document.getElementById("portal")!
      )
    : null;
}
