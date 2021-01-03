import React from "react";

import { AuthUser } from "@/services/user";

export interface MoreAdminProps {
  user: AuthUser;
}

const MoreAdmin: React.FC<MoreAdminProps> = () => {
  return <>More...</>;
};

export default MoreAdmin;
