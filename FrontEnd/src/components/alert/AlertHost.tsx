import { useEffect, useState } from "react";
import classNames from "classnames";

import { ThemeColor, useC, Text } from "../common";
import IconButton from "../button/IconButton";

import { alertService, AlertInfoWithId } from "./AlertService";

import "./alert.css";

interface AutoCloseAlertProps {
  color: ThemeColor;
  message: Text;
  onDismiss?: () => void;
  onIn?: () => void;
  onOut?: () => void;
}

function Alert({
  color,
  message,
  onDismiss,
  onIn,
  onOut,
}: AutoCloseAlertProps) {
  const c = useC();

  return (
    <div
      className={classNames("cru-alert", `cru-theme-${color}`)}
      onPointerEnter={onIn}
      onPointerLeave={onOut}
    >
      <div className="cru-alert-message">{c(message)}</div>
      <IconButton
        icon="x"
        color="danger"
        className="cru-alert-close-button"
        onClick={onDismiss}
      />
    </div>
  );
}

export default function AlertHost() {
  const [alerts, setAlerts] = useState<AlertInfoWithId[]>([]);

  useEffect(() => {
    alertService.registerListener(setAlerts);

    return () => {
      alertService.unregisterListener(setAlerts);
      alert;
    };
  }, []);

  return (
    <div className="alert-container">
      {alerts.map((alert) => {
        return (
          <Alert
            key={alert.id}
            message={alert.message}
            color={alert.color ?? "primary"}
            onIn={() => {
              alertService.clearDismissTimer(alert.id);
            }}
            onOut={() => {
              alertService.resetDismissTimer(alert.id);
            }}
            onDismiss={() => {
              alertService.dismiss(alert.id);
            }}
          />
        );
      })}
    </div>
  );
}
