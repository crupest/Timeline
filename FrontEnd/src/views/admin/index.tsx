import { lazy } from "react";

const Admin = lazy(
  () => import(/* webpackChunkName: "admin" */ "./Admin")
);

export default Admin;
