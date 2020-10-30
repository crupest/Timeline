import React from "react";
import clsx from "clsx";

import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";
import SyncStatusBadge from "../timeline-common/SyncStatusBadge";
import CollapseButton from "../timeline-common/CollapseButton";

const InfoCardTemplate: React.FC<
  Pick<
    TimelineCardComponentProps<"">,
    "collapse" | "toggleCollapse" | "syncStatus" | "className"
  > & { children: React.ReactElement[] }
> = ({ collapse, toggleCollapse, syncStatus, className, children }) => {
  return (
    <div className={clsx("cru-card p-2 clearfix", className)}>
      <div className="float-right d-flex align-items-center">
        <SyncStatusBadge status={syncStatus} className="mr-2" />
        <CollapseButton collapse={collapse} onClick={toggleCollapse} />
      </div>

      <div style={{ display: collapse ? "none" : "block" }}>{children}</div>
    </div>
  );
};

export default InfoCardTemplate;
