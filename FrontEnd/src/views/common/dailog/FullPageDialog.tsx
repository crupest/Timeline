import React from "react";
import { createPortal } from "react-dom";
import classnames from "classnames";

import "./FullPageDialog.css";

export interface FullPageDialogProps {
  show: boolean;
  onBack: () => void;
  contentContainerClassName?: string;
}

const FullPageDialog: React.FC<FullPageDialogProps> = ({
  show,
  onBack,
  children,
  contentContainerClassName,
}) => {
  return createPortal(
    <div
      className="cru-full-page"
      style={{ display: show ? undefined : "none" }}
    >
      <div className="cru-full-page-top-bar">
        <i
          className="icon-button bi-arrow-left ms-3 cru-full-page-back-button"
          onClick={onBack}
        />
      </div>
      <div
        className={classnames(
          "cru-full-page-content-container",
          contentContainerClassName
        )}
      >
        {children}
      </div>
    </div>,
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    document.getElementById("portal")!
  );
};

export default FullPageDialog;
