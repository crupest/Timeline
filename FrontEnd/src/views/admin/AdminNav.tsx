import React from "react";
import { useParams } from "react-router-dom";

import Tabs from "../common/tab/Tabs";

const AdminNav: React.FC<{ className?: string }> = ({ className }) => {
  const params = useParams();

  const name = params.name;

  return (
    <Tabs
      className={className}
      activeTabName={name}
      tabs={[
        {
          name: "users",
          text: "admin:nav.users",
          link: "/admin/users",
        },
        {
          name: "more",
          text: "admin:nav.more",
          link: "/admin/more",
        },
      ]}
    />
  );
};

export default AdminNav;
