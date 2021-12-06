import React from "react";
import { Route, Routes } from "react-router-dom";
import { useTranslation } from "react-i18next";

import AdminNav from "./AdminNav";
import UserAdmin from "./UserAdmin";
import MoreAdmin from "./MoreAdmin";

import "./index.css";

const Admin: React.FC = () => {
  useTranslation("admin");

  return (
    <>
      <div className="container">
        <AdminNav className="mt-2" />
        <Routes>
          <Route index element={<UserAdmin />} />
          <Route path="/admin/user" element={<UserAdmin />} />
          <Route path="/admin/more" element={<MoreAdmin />} />
        </Routes>
      </div>
    </>
  );
};

export default Admin;
