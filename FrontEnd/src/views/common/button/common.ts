import React from "react";
import classNames from "classnames";
import { TFunction } from "i18next";

import { convertI18nText, I18nText } from "@/common";
import { PaletteColorType } from "@/palette";

export type CommonButtonProps = {
  text?: I18nText;
  color?: PaletteColorType;
} & React.ButtonHTMLAttributes<HTMLButtonElement>;

export function calculateProps(
  props: CommonButtonProps,
  buttonClassName: string,
  t: TFunction
): {
  children: React.ReactNode;
  newProps: React.ButtonHTMLAttributes<HTMLButtonElement>;
} {
  const { text, color, className, children, ...otherProps } = props;
  const newProps = {
    className: classNames(buttonClassName, color ?? "primary", className),
    ...otherProps,
  };

  return {
    children: text != null ? convertI18nText(text, t) : children,
    newProps: newProps,
  };
}
