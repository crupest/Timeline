import * as React from "react";
import { useTranslation } from "react-i18next";
import classNames from "classnames";

import { convertI18nText, I18nText } from "@/common";
import { PaletteColorType } from "@/palette";

import "./FlatButton.css";

function _FlatButton(
  props: {
    color?: PaletteColorType;
    text?: I18nText;
  } & React.ComponentPropsWithoutRef<"button">,
  ref: React.ForwardedRef<HTMLButtonElement>
): React.ReactElement | null {
  const { t } = useTranslation();

  const { color, text, className, children, ...otherProps } = props;

  if (text != null && children != null) {
    console.warn("You can't set both text and children props.");
  }

  return (
    <button
      ref={ref}
      className={classNames(
        "cru-" + (color ?? "primary"),
        "cru-flat-button",
        className
      )}
      {...otherProps}
    >
      {text != null ? convertI18nText(text, t) : children}
    </button>
  );
}

const FlatButton = React.forwardRef(_FlatButton);
export default FlatButton;
