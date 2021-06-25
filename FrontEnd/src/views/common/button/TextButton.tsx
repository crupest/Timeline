import React from "react";

import { CommonButtonProps } from "./common";
import Button from "./Button";

import "./TextButton.css";

function _TextButton(
  props: CommonButtonProps,
  ref: React.ForwardedRef<HTMLButtonElement>
): React.ReactElement | null {
  return (
    <Button ref={ref} customButtonClassName="cru-flat-button" {...props} />
  );
}

const TextButton = React.forwardRef(_TextButton);
export default TextButton;
