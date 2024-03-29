import { useEffect, useState } from "react";
import classNames from "classnames";

import { ThemeColor, useC, I18nText } from "../common";
import IconButton from "../button/IconButton";

import { alertService, AlertInfoWithId } from "./AlertService";

import "./alert.css";

interface AutoCloseAlertProps {
  color: ThemeColor;
  message: I18nText;
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
    const listener = (alerts: AlertInfoWithId[]) => {
      setAlerts(alerts);
    };

    alertService.registerListener(listener);

    return () => {
      alertService.unregisterListener(listener);
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
