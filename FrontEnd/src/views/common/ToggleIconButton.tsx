import React from "react";
import classnames from "classnames";

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
      className={classnames(
        state ? trueIconClassName : falseIconClassName,
        "icon-button",
        className
      )}
      {...otherProps}
    />
  );
};

export default ToggleIconButton;
