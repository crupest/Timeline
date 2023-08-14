import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import { ThemeColor } from "./common";
import "./Card.css";

interface CardProps extends ComponentPropsWithoutRef<"div"> {
  containerRef?: Ref<HTMLDivElement>;
  color?: ThemeColor;
}

export default function Card({
  color,
  className,
  children,
  containerRef,
  ...otherProps
}: CardProps) {
  return (
    <div
      ref={containerRef}
      className={classNames("cru-card", `cru-card-${color ?? "primary"}`, className)}
      {...otherProps}
    >
      {children}
    </div>
  );
}
