import * as React from "react";
import classNames from "classnames";
import { useTranslation } from "react-i18next";

import { convertI18nText, I18nText, ThemeColor } from "@/common";

import Spinner from "../Spinner";

interface LoadingButtonProps extends React.ComponentPropsWithoutRef<"button"> {
  color?: ThemeColor;
  text?: I18nText;
  loading?: boolean;
}

function LoadingButton(props: LoadingButtonProps): JSX.Element {
  const { t } = useTranslation();

  const { color, text, loading, className, children, ...otherProps } = props;

  if (text != null && children != null) {
    console.warn("You can't set both text and children props.");
  }

  return (
    <button
      className={classNames(
        "cru-" + (color ?? "primary"),
        "cru-button outline",
        className,
      )}
      {...otherProps}
    >
      {text != null ? convertI18nText(text, t) : children}
      {loading && <Spinner />}
    </button>
  );
}

export default LoadingButton;
