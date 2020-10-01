import React from "react";
import clsx from "clsx";

import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";
import SyncStatusBadge from "../timeline-common/SyncStatusBadge";
import CollapseButton from "../timeline-common/CollapseButton";

const InfoCardTemplate: React.FC<
  Pick<
    TimelineCardComponentProps<"">,
    "toggleCollapse" | "syncStatus" | "className"
  > & { children: React.ReactElement[] }
> = ({ toggleCollapse, syncStatus, className, children }) => {
  return (
    <div className={clsx("cru-card p-2 clearfix", className)}>
      <div className="float-right d-flex align-items-center">
        <SyncStatusBadge status={syncStatus} className="mr-2" />
        <CollapseButton collapse={false} onClick={toggleCollapse} />
      </div>

      <div>{children}</div>
    </div>
  );
};

export default InfoCardTemplate;
