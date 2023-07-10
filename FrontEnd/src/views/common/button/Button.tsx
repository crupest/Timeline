import * as React from "react";
import classNames from "classnames";
import { useTranslation } from "react-i18next";

import { convertI18nText, I18nText } from "@/common";
import { PaletteColorType } from "@/palette";

import "./Button.css";

function _Button(
  props: {
    color?: PaletteColorType;
    text?: I18nText;
    outline?: boolean;
  } & React.ComponentPropsWithoutRef<"button">,
  ref: React.ForwardedRef<HTMLButtonElement>
): JSX.Element {
  const { t } = useTranslation();

  const { color, text, outline, className, children, ...otherProps } = props;

  if (text != null && children != null) {
    console.warn("You can't set both text and children props.");
  }

  return (
    <button
      ref={ref}
      className={classNames(
        "cru-" + (color ?? "primary"),
        "cru-button",
        outline && "outline",
        className
      )}
      {...otherProps}
    >
      {text != null ? convertI18nText(text, t) : children}
    </button>
  );
}

const Button = React.forwardRef(_Button);
export default Button;
