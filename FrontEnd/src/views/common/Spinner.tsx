import React from "react";
import classnames from "classnames";

import { PaletteColorType } from "@/palette";

import "./Spinner.css";

export interface SpinnerProps {
  size?: "sm" | "md" | "lg" | number | string;
  color?: PaletteColorType;
  className?: string;
  style?: React.CSSProperties;
}

export default function Spinner(
  props: SpinnerProps
): React.ReactElement | null {
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
  const calculatedColor = color ?? "primary";

  return (
    <span
      className={classnames(
        "cru-spinner",
        `cru-color-${calculatedColor}`,
        className
      )}
      style={{ width: calculatedSize, height: calculatedSize, ...style }}
    />
  );
}
