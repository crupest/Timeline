import React from "react";
import { useTranslation } from "react-i18next";

import { convertI18nText, I18nText } from "@/common";
import { PaletteColorType } from "@/palette";

import "./FlatButton.css";
import classNames from "classnames";

function _FlatButton(
  {
    text,
    color,
    onClick,
  }: {
    text: I18nText;
    color?: PaletteColorType;
    onClick?: () => void;
  },
  ref: React.ForwardedRef<HTMLButtonElement>
): React.ReactElement | null {
  const { t } = useTranslation();

  return (
    <button
      ref={ref}
      className={classNames("cru-flat-button", color ?? "primary")}
      onClick={onClick}
    >
      {convertI18nText(text, t)}
    </button>
  );
}

const FlatButton = React.forwardRef(_FlatButton);
export default FlatButton;
