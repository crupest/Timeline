import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import { Text, useC, ThemeColor } from "../common";

import "./Button.css";

interface ButtonProps extends ComponentPropsWithoutRef<"button"> {
  color?: ThemeColor;
  text?: Text;
  outline?: boolean;
  buttonRef?: Ref<HTMLButtonElement> | null;
}

export default function Button(props: ButtonProps) {
  const {
    buttonRef,
    color,
    text,
    outline,
    className,
    children,
    ...otherProps
  } = props;

  if (text != null && children != null) {
    console.warn("You can't set both text and children props.");
  }

  const c = useC();

  return (
    <button
      ref={buttonRef}
      className={classNames(
        "cru-button",
        `cru-clickable-${color ?? "primary"}`,
        outline && "outline",
        className,
      )}
      {...otherProps}
    >
      {text != null ? c(text) : children}
    </button>
  );
}
