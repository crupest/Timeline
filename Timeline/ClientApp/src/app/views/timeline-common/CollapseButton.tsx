import React from "react";
import clsx from "clsx";
import Svg from "react-inlinesvg";
import arrowsAngleContractIcon from "bootstrap-icons/icons/arrows-angle-contract.svg";
import arrowsAngleExpandIcon from "bootstrap-icons/icons/arrows-angle-expand.svg";

const CollapseButton: React.FC<{
  collapse: boolean;
  onClick: () => void;
  className?: string;
  style?: React.CSSProperties;
}> = ({ collapse, onClick, className, style }) => {
  return (
    <Svg
      src={collapse ? arrowsAngleExpandIcon : arrowsAngleContractIcon}
      onClick={onClick}
      className={clsx("text-primary icon-button", className)}
      style={style}
    />
  );
};

export default CollapseButton;
