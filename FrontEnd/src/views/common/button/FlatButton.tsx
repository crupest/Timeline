import React from "react";

import { CommonButtonProps } from "./common";
import Button from "./Button";

import "./FlatButton.css";

function _FlatButton(
  props: CommonButtonProps,
  ref: React.ForwardedRef<HTMLButtonElement>
): React.ReactElement | null {
  return (
    <Button ref={ref} customButtonClassName="cru-flat-button" {...props} />
  );
}

const FlatButton = React.forwardRef(_FlatButton);
export default FlatButton;
