import { ComponentPropsWithoutRef } from "react";
import classNames from "classnames";

import { ThemeColor } from "./common";

import "./Icon.css";

interface IconButtonProps extends ComponentPropsWithoutRef<"i"> {
  icon: string;
  color?: ThemeColor;
  size?: string | number;
}

export default function Icon(props: IconButtonProps) {
  const { icon, color, size, style, className, ...otherProps } = props;

  return (
    <i
      style={size != null ? { ...style, fontSize: size } : style}
      className={classNames(
        `cru-theme-${color ?? "primary"}`,
        `bi-${icon} cru-icon`,
        className,
      )}
      {...otherProps}
    />
  );
}
