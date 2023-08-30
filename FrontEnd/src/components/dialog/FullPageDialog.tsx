import * as React from "react";
import { createPortal } from "react-dom";
import classnames from "classnames";
import { CSSTransition } from "react-transition-group";

import "./FullPageDialog.css";
import IconButton from "../button/IconButton";

export interface FullPageDialogProps {
  show: boolean;
  onBack: () => void;
  contentContainerClassName?: string;
  children: React.ReactNode;
}

const FullPageDialog: React.FC<FullPageDialogProps> = ({
  show,
  onBack,
  children,
  contentContainerClassName,
}) => {
  return createPortal(
    <CSSTransition
      mountOnEnter
      unmountOnExit
      in={show}
      timeout={300}
      classNames="cru-full-page"
    >
      <div className="cru-full-page">
        <div className="cru-full-page-top-bar">
          <IconButton
            icon="arrow-left"
            className="ms-3 cru-full-page-back-button"
            onClick={onBack}
          />
        </div>
        <div
          className={classnames(
            "cru-full-page-content-container",
            contentContainerClassName,
          )}
        >
          {children}
        </div>
      </div>
    </CSSTransition>,
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    document.getElementById("portal")!,
  );
};

export default FullPageDialog;
