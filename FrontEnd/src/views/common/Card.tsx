import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import { ThemeColor } from "./common";
import "./Card.css";

interface CardProps extends ComponentPropsWithoutRef<"div"> {
  containerRef?: Ref<HTMLDivElement>;
  color?: ThemeColor;
  border?: "color" | "none";
  background?: "color" | "solid" | "grayscale" | "none";
}

export default function Card({
  color,
  background,
  border,
  className,
  children,
  containerRef,
  ...otherProps
}: CardProps) {
  return (
    <div
      ref={containerRef}
      className={classNames(
        "cru-card",
        `cru-card-${color ?? "primary"}`,
        `cru-card-border-${border ?? "color"}`,
        `cru-card-background-${background ?? "solid"}`,
        className,
      )}
      {...otherProps}
    >
      {children}
    </div>
  );
}
