import React from "react";

import { AuthUser } from "@/services/user";

export interface HighlightTimelineAdminProps {
  user: AuthUser;
}

const HighlightTimelineAdmin: React.FC<HighlightTimelineAdminProps> = () => {
  return <>This is highlight timeline administration page.</>;
};

export default HighlightTimelineAdmin;
