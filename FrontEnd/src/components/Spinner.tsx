import { CSSProperties, ComponentPropsWithoutRef } from "react";
import classNames from "classnames";

import "./Spinner.css";

const sizeMap: Record<string, string> = {
  sm: "18px",
  md: "30px",
  lg: "42px",
};

function calculateSize(size: SpinnerProps["size"]) {
  if (size == null) {
    return "1em";
  }
  if (typeof size === "number") {
    return size;
  }
  if (size in sizeMap) {
    return sizeMap[size];
  }
  return size;
}

export interface SpinnerProps extends ComponentPropsWithoutRef<"span"> {
  size?: number | string;
  className?: string;
  style?: CSSProperties;
}

export default function Spinner(props: SpinnerProps) {
  const { size, className, style, ...otherProps } = props;
  const calculatedSize = calculateSize(size);

  return (
    <span
      className={classNames("cru-spinner", className)}
      style={{
        width: calculatedSize,
        height: calculatedSize,
        ...style,
      }}
      {...otherProps}
    />
  );
}
