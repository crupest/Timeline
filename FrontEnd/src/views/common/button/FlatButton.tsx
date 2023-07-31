import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import { Text, useC, ThemeColor } from "../common";

import "./FlatButton.css";

interface FlatButtonProps extends ComponentPropsWithoutRef<"button"> {
  color?: ThemeColor;
  text?: Text;
  buttonRef?: Ref<HTMLButtonElement> | null;
}

export default function FlatButton(props: FlatButtonProps) {
  const { color, text, className, children, buttonRef, ...otherProps } = props;

  if (text != null && children != null) {
    console.warn("You can't set both text and children props.");
  }

  const c = useC();

  return (
    <button
      ref={buttonRef}
      className={classNames(
        color && `cru-${color}`,
        "cru-flat-button",
        className,
      )}
      {...otherProps}
    >
      {text != null ? c(text) : children}
    </button>
  );
}
