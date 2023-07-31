import classNames from "classnames";

import { I18nText, ThemeColor, useC } from "../common";

import Spinner from "../Spinner";

import "./LoadingButton.css";

interface LoadingButtonProps extends React.ComponentPropsWithoutRef<"button"> {
  color?: ThemeColor;
  text?: I18nText;
  loading?: boolean;
}

export default function LoadingButton(props: LoadingButtonProps) {
  const c = useC();

  const { color, text, loading, disabled, className, children, ...otherProps } =
    props;

  if (text != null && children != null) {
    console.warn("You can't set both text and children props.");
  }

  return (
    <button
      disabled={disabled || loading}
      className={classNames(
        color && `cru-${color}`,
        "cru-button outline cru-loading-button",
        className,
      )}
      {...otherProps}
    >
      {text != null ? c(text) : children}
      {loading && <Spinner className="cru-loading-button-spinner" />}
    </button>
  );
}
