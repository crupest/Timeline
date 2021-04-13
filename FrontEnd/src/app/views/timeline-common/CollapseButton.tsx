import React from "react";
import classnames from "classnames";

const CollapseButton: React.FC<{
  collapse: boolean;
  onClick: () => void;
  className?: string;
  style?: React.CSSProperties;
}> = ({ collapse, onClick, className, style }) => {
  return (
    <i
      onClick={onClick}
      className={classnames(
        collapse ? "bi-arrows-angle-expand" : "bi-arrows-angle-contract",
        "text-primary icon-button",
        className
      )}
      style={style}
    />
  );
};

export default CollapseButton;
