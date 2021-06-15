import React from "react";
import classnames from "classnames";
import { HubConnectionState } from "@microsoft/signalr";
import { useTranslation } from "react-i18next";

export interface ConnectionStatusBadgeProps {
  status: HubConnectionState;
  className?: string;
  style?: React.CSSProperties;
}

const classNameMap: Record<HubConnectionState, string> = {
  Connected: "success",
  Connecting: "warning",
  Disconnected: "danger",
  Disconnecting: "warning",
  Reconnecting: "warning",
};

const ConnectionStatusBadge: React.FC<ConnectionStatusBadgeProps> = (props) => {
  const { status, className, style } = props;

  const { t } = useTranslation();

  return (
    <div
      className={classnames(
        "connection-status-badge",
        classNameMap[status],
        className
      )}
      style={style}
    >
      {t(`connectionState.${status}`)}
    </div>
  );
};

export default ConnectionStatusBadge;
