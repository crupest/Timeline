import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import { ThemeColor } from "./common";
import "./Card.css";

interface CardProps extends ComponentPropsWithoutRef<"div"> {
  containerRef?: Ref<HTMLDivElement>;
  color?: ThemeColor;
  noBackground?: boolean;
}

export default function Card({
  color,
  noBackground,
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
        noBackground && "cru-card-no-background",
        className,
      )}
      {...otherProps}
    >
      {children}
    </div>
  );
}
