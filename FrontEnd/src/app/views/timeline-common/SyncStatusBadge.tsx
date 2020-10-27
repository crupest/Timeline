import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";

import { UiLogicError } from "@/common";

export type TimelineSyncStatus = "syncing" | "synced" | "offline";

const SyncStatusBadge: React.FC<{
  status: TimelineSyncStatus;
  style?: React.CSSProperties;
  className?: string;
}> = ({ status, style, className }) => {
  const { t } = useTranslation();

  return (
    <div style={style} className={clsx("timeline-sync-state-badge", className)}>
      {(() => {
        switch (status) {
          case "syncing": {
            return (
              <>
                <span className="timeline-sync-state-badge-pin bg-warning" />
                <span className="text-warning">
                  {t("timeline.postSyncState.syncing")}
                </span>
              </>
            );
          }
          case "synced": {
            return (
              <>
                <span className="timeline-sync-state-badge-pin bg-success" />
                <span className="text-success">
                  {t("timeline.postSyncState.synced")}
                </span>
              </>
            );
          }
          case "offline": {
            return (
              <>
                <span className="timeline-sync-state-badge-pin bg-danger" />
                <span className="text-danger">
                  {t("timeline.postSyncState.offline")}
                </span>
              </>
            );
          }
          default:
            throw new UiLogicError("Unknown sync state.");
        }
      })()}
    </div>
  );
};

export default SyncStatusBadge;
