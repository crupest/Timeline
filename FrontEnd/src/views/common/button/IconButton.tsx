import classNames from "classnames";
import React from "react";

export type IconButtonProps = {
  icon: string;
  color?: string;
  large?: boolean;
} & React.ComponentPropsWithRef<"i">;

export default function IconButton(props: IconButtonProps): JSX.Element {
  const { icon, color, className, large, ...otherProps } = props;

  return (
    <i
      className={classNames(
        "cru-icon-button",
        large && "large",
        "bi-" + icon,
        color ? "cru-" + color : "cru-primary",
        className
      )}
      {...otherProps}
    />
  );
}
