import { CSSProperties } from "react";
import classnames from "classnames";

import { ThemeColor } from "./common";

import "./Spinner.css";

export interface SpinnerProps {
  size?: "sm" | "md" | "lg" | number | string;
  color?: ThemeColor;
  className?: string;
  style?: CSSProperties;
}

export default function Spinner(props: SpinnerProps) {
  const { size, color, className, style } = props;
  const calculatedSize =
    size === "sm"
      ? "18px"
      : size === "md"
      ? "30px"
      : size === "lg"
      ? "42px"
      : typeof size === "number"
      ? size
      : size == null
      ? "20px"
      : size;

  return (
    <span
      className={classnames("cru-spinner", color && `cru-${color}`, className)}
      style={{ width: calculatedSize, height: calculatedSize, ...style }}
    />
  );
}
