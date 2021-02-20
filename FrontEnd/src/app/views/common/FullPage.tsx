import React from "react";

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
      <div className={contentContainerClassName}>{children}</div>
    </div>
  );
};

export default FullPage;
