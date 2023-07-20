import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import "./Card.css";

interface CardProps extends ComponentPropsWithoutRef<"div"> {
  containerRef?: Ref<HTMLDivElement> | null;
}

export default function Card({
  className,
  children,
  containerRef,
  ...otherProps
}: CardProps) {
  return (
    <div
      ref={containerRef}
      className={classNames("cru-card", className)}
      {...otherProps}
    >
      {children}
    </div>
  );
}
