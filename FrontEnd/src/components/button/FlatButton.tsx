import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import { I18nText, useC, ClickableColor } from "../common";

import "./FlatButton.css";

interface FlatButtonProps extends ComponentPropsWithoutRef<"button"> {
  color?: ClickableColor;
  text?: I18nText;
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
        "cru-flat-button",
        `cru-clickable-${color ?? "primary"}`,
        className,
      )}
      {...otherProps}
    >
      {text != null ? c(text) : children}
    </button>
  );
}
