import React from "react";
import { useRouteMatch } from "react-router";

import Tabs from "../common/tab/Tabs";

const AdminNav: React.FC = () => {
  const match = useRouteMatch<{ name: string }>();

  const name = match.params.name;

  return (
    <Tabs
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
