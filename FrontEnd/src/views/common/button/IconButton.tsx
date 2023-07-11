import { ComponentPropsWithoutRef } from "react";
import classNames from "classnames";

import { PaletteColorType } from "@/palette";

import "./IconButton.css";

interface IconButtonProps extends ComponentPropsWithoutRef<"i"> {
  icon: string;
  color?: PaletteColorType;
  large?: boolean;
}

export default function IconButton(props: IconButtonProps) {
  const { icon, color, className, large, ...otherProps } = props;

  return (
    <i
      className={classNames(
        "cru-icon-button",
        large && "large",
        "bi-" + icon,
        color ? "cru-" + color : "cru-primary",
        className,
      )}
      {...otherProps}
    />
  );
}
