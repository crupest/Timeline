import React from "react";
import { createPortal } from "react-dom";
import classnames from "classnames";
import { CSSTransition } from "react-transition-group";

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
    <CSSTransition in={show} timeout={300} classNames="cru-full-page">
      <div className="cru-full-page">
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
      </div>
    </CSSTransition>,
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    document.getElementById("portal")!
  );
};

export default FullPageDialog;
