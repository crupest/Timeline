import classNames from "classnames";
import { HubConnectionState } from "@microsoft/signalr";

import { useC }from '~/src/components/common';

import "./ConnectionStatusBadge.css";

interface ConnectionStatusBadgeProps {
  status: HubConnectionState;
  className?: string;
}

const classNameMap: Record<HubConnectionState, string> = {
  Connected: "success",
  Connecting: "warning",
  Disconnected: "danger",
  Disconnecting: "warning",
  Reconnecting: "warning",
};

export default function ConnectionStatusBadge({status, className}: ConnectionStatusBadgeProps) {
  const c = useC();

  return (
    <div
      className={classNames(
        "connection-status-badge",
        classNameMap[status],
        className
      )}
    >
      {c(`connectionState.${status}`)}
    </div>
  );
};

