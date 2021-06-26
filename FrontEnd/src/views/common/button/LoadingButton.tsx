import React from "react";

import { CommonButtonProps } from "./common";
import Button from "./Button";
import Spinner from "../Spinner";

const LoadingButton: React.FC<{ loading?: boolean } & CommonButtonProps> = ({
  loading,
  disabled,
  color,
  ...otherProps
}) => {
  return (
    <Button color={color} disabled={disabled || loading} {...otherProps}>
      {otherProps.children}
      {loading ? <Spinner color={color} /> : null}
    </Button>
  );
};

export default LoadingButton;
