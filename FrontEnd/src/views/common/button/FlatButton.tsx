import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import { I18nText, useC } from "@/common";
import { PaletteColorType } from "@/palette";

import "./FlatButton.css";

interface FlatButtonProps extends ComponentPropsWithoutRef<"button"> {
  color?: PaletteColorType;
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
        "cru-" + (color ?? "primary"),
        "cru-flat-button",
        className,
      )}
      {...otherProps}
    >
      {text != null ? c(text) : children}
    </button>
  );
}
