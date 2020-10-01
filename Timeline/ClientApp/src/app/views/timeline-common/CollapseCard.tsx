import React from "react";
import clsx from "clsx";

import SyncStatusBadge, {
  TimelineSyncStatus,
} from "../timeline-common/SyncStatusBadge";
import CollapseButton from "../timeline-common/CollapseButton";

const CollapseCard: React.FC<{
  className?: string;
  syncStatus: TimelineSyncStatus;
  toggleCollapse: () => void;
  visible: boolean;
}> = ({ className, syncStatus, toggleCollapse, visible }) => {
  return (
    <div
      style={{ visibility: visible ? "visible" : "hidden" }}
      className={clsx("cru-card p-2", className)}
    >
      <div className="d-flex align-items-center">
        <SyncStatusBadge status={syncStatus} className="mr-2" />
        <CollapseButton collapse onClick={toggleCollapse} />
      </div>
    </div>
  );
};

export default CollapseCard;
