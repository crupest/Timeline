import classNames from "classnames";
import React from "react";

import "./Card.css";

function _Card(
  {
    className,
    children,
    ...otherProps
  }: React.PropsWithChildren<React.HTMLAttributes<HTMLDivElement>>,
  ref: React.ForwardedRef<HTMLDivElement>
): React.ReactElement | null {
  return (
    <div
      ref={ref}
      className={classNames("cru-card", className)}
      {...otherProps}
    >
      {children}
    </div>
  );
}

const Card = React.forwardRef(_Card);

export default Card;
