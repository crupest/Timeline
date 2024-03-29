import { useLocation } from "react-router-dom";

import Tabs from "../common/tab/Tabs";

export function AdminNav({ className }: { className?: string }) {
  const location = useLocation();
  const name = location.pathname.split("/")[2] ?? "user";

  return (
    <Tabs
      className={className}
      activeTabName={name}
      tabs={[
        {
          name: "user",
          text: "admin:nav.users",
          link: "/admin/user",
        },
        {
          name: "more",
          text: "admin:nav.more",
          link: "/admin/more",
        },
      ]}
    />
  );
}

export default AdminNav;
