import React from "react";
import classnames from "classnames";

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
  return (
    <div
      className="cru-full-page"
      style={{ display: show ? undefined : "none" }}
    >
      <div className="cru-full-page-top-bar">
        <i
          className="icon-button bi-arrow-left text-white ms-3"
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
  );
};

export default FullPageDialog;
