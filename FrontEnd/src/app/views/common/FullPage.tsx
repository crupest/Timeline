import React from "react";
import clsx from "clsx";

export interface FullPageProps {
  show: boolean;
  onBack: () => void;
  contentContainerClassName?: string;
}

const FullPage: React.FC<FullPageProps> = ({
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
          className="icon-button bi-arrow-left text-white ml-3"
          onClick={onBack}
        />
      </div>
      <div
        className={clsx(
          "cru-full-page-content-container",
          contentContainerClassName
        )}
      >
        {children}
      </div>
    </div>
  );
};

export default FullPage;
