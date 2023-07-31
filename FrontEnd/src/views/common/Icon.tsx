import { ComponentPropsWithoutRef } from "react";
import classNames from "classnames";

import { ThemeColor } from "./common";

import "./Icon.css";

interface IconButtonProps extends ComponentPropsWithoutRef<"i"> {
  icon: string;
  color?: ThemeColor | "on-surface";
  size?: string | number;
}

export default function Icon(props: IconButtonProps) {
  const { icon, color, size, style, className, ...otherProps } = props;

  const colorName = color === "on-surface" ? "surface-on" : color ?? "primary";

  return (
    <i
      style={size != null ? { ...style, fontSize: size } : style}
      className={classNames(
        `bi-${icon} cru-${colorName}-color cru-icon`,
        className,
      )}
      {...otherProps}
    />
  );
}
