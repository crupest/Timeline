import { CSSProperties } from "react";

import IconButton from "~src/components/button/IconButton";

export default function CollapseButton({
  collapse,
  onClick,
  className,
  style,
}: {
  collapse: boolean;
  onClick: () => void;
  className?: string;
  style?: CSSProperties;
}) {
  return (
    <IconButton
      color="primary"
      icon={collapse ? "arrows-angle-expand" : "arrows-angle-contract"}
      onClick={onClick}
      className={className}
      style={style}
    />
  );
}
