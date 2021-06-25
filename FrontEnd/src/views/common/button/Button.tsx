import React from "react";
import { useTranslation } from "react-i18next";

import { calculateProps, CommonButtonProps } from "./common";

import "./Button.css";

function _Button(
  props: CommonButtonProps & { customButtonClassName?: string },
  ref: React.ForwardedRef<HTMLButtonElement>
): React.ReactElement | null {
  const { t } = useTranslation();

  const { customButtonClassName, ...otherProps } = props;

  const { newProps, children } = calculateProps(
    otherProps,
    customButtonClassName ?? "cru-button",
    t
  );

  return (
    <button ref={ref} {...newProps}>
      {children}
    </button>
  );
}

const Button = React.forwardRef(_Button);
export default Button;
