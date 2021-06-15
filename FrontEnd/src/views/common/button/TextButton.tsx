import React from "react";
import { useTranslation } from "react-i18next";
import classNames from "classnames";

import { convertI18nText, I18nText } from "@/common";
import { PaletteColorType } from "@/palette";

import "./TextButton.css";

function _TextButton(
  {
    text,
    color,
    onClick,
    className,
    style,
  }: {
    text: I18nText;
    color?: PaletteColorType;
    onClick?: () => void;
    className?: string;
    style?: React.CSSProperties;
  },
  ref: React.ForwardedRef<HTMLButtonElement>
): React.ReactElement | null {
  const { t } = useTranslation();

  return (
    <button
      ref={ref}
      className={classNames("cru-text-button", color ?? "primary", className)}
      onClick={onClick}
      style={style}
    >
      {convertI18nText(text, t)}
    </button>
  );
}

const TextButton = React.forwardRef(_TextButton);
export default TextButton;
