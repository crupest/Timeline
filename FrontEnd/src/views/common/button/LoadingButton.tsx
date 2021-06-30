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
    <Button
      color={color}
      outline
      disabled={disabled || loading}
      {...otherProps}
    >
      {otherProps.children}
      {loading ? (
        <Spinner className="cru-align-text-bottom ms-1" color={color} />
      ) : null}
    </Button>
  );
};

export default LoadingButton;
