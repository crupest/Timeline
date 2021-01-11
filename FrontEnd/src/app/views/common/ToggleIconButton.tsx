import React from "react";
import clsx from "clsx";

export interface ToggleIconButtonProps
  extends React.HTMLAttributes<HTMLElement> {
  state: boolean;
  trueIconClassName: string;
  falseIconClassName: string;
}

const ToggleIconButton: React.FC<ToggleIconButtonProps> = ({
  state,
  className,
  trueIconClassName,
  falseIconClassName,
  ...otherProps
}) => {
  return (
    <i
      className={clsx(
        state ? trueIconClassName : falseIconClassName,
        "icon-button",
        className
      )}
      {...otherProps}
    />
  );
};

export default ToggleIconButton;
