import { ComponentPropsWithoutRef } from "react";
import classNames from "classnames";

import { ThemeColor } from "../common";

import "./IconButton.css";

interface IconButtonProps extends ComponentPropsWithoutRef<"i"> {
  icon: string;
  color?: ThemeColor;
  large?: boolean;
  disabled?: boolean; // TODO: Not implemented
}

export default function IconButton(props: IconButtonProps) {
  const { icon, color, className, large, ...otherProps } = props;

  return (
    <button
      className={classNames(
        "cru-icon-button",
        `cru-button-${color ?? "primary"}`,
        large && "large",
        "bi-" + icon,
        className,
      )}
      {...otherProps}
    />
  );
}
