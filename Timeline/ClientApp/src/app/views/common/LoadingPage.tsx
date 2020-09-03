import React from "react";
import { Spinner } from "react-bootstrap";

const LoadingPage: React.FC = () => {
  return (
    <div className="position-fixed w-100 h-100 d-flex justify-content-center align-items-center">
      <Spinner variant="primary" animation="border" />
    </div>
  );
};

export default LoadingPage;
