import * as React from "react";

import IconButton from "../common/button/IconButton";

const CollapseButton: React.FC<{
  collapse: boolean;
  onClick: () => void;
  className?: string;
  style?: React.CSSProperties;
}> = ({ collapse, onClick, className, style }) => {
  return (
    <IconButton
      icon={collapse ? "arrows-angle-expand" : "arrows-angle-contract"}
      onClick={onClick}
      className={className}
      style={style}
    />
  );
};

export default CollapseButton;
