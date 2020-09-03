import React from "react";
import { Button, ButtonProps, Spinner } from "react-bootstrap";

const LoadingButton: React.FC<{ loading?: boolean } & ButtonProps> = ({
  loading,
  variant,
  ...otherProps
}) => {
  return (
    <Button
      variant={variant != null ? `outline-${variant}` : "outline-primary"}
      disabled={loading}
      {...otherProps}
    >
      {otherProps.children}
      {loading ? (
        <Spinner
          className="ml-1"
          variant={variant}
          animation="grow"
          size="sm"
        />
      ) : null}
    </Button>
  );
};

export default LoadingButton;
