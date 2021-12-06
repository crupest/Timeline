import * as React from "react";

const Admin = React.lazy(
  () => import(/* webpackChunkName: "admin" */ "./Admin")
);

export default Admin;
