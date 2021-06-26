import React from "react";

import Spinner from "./Spinner";

const LoadingPage: React.FC = () => {
  return (
    <div className="position-fixed w-100 h-100 d-flex justify-content-center align-items-center">
      <Spinner />
    </div>
  );
};

export default LoadingPage;
