import React from "react";
import clsx from "clsx";

import { BootstrapThemeColor } from "@/common";

export interface FlatButtonProps {
  variant?: BootstrapThemeColor | string;
  disabled?: boolean;
  className?: string;
  style?: React.CSSProperties;
  onClick?: () => void;
}

const FlatButton: React.FC<FlatButtonProps> = (props) => {
  const { disabled, className, style } = props;
  const variant = props.variant ?? "primary";

  const onClick = disabled ? undefined : props.onClick;

  return (
    <div
      className={clsx(
        "flat-button",
        variant,
        disabled ? "disabled" : null,
        className
      )}
      style={style}
      onClick={onClick}
    >
      {props.children}
    </div>
  );
};

export default FlatButton;
