import { ComponentPropsWithoutRef } from "react";
import classNames from "classnames";

import { ClickableColor } from "../common";

import "./IconButton.css";

interface IconButtonProps extends ComponentPropsWithoutRef<"i"> {
  icon: string;
  color?: ClickableColor;
  large?: boolean;
  disabled?: boolean; // TODO: Not implemented
}

export default function IconButton(props: IconButtonProps) {
  const { icon, color, className, large, ...otherProps } = props;

  return (
    <button
      className={classNames(
        "cru-icon-button",
        `cru-clickable-${color ?? "grayscale"}`,
        large && "large",
        "bi-" + icon,
        className,
      )}
      {...otherProps}
    />
  );
}
