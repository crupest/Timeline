import React from "react";
import without from "lodash/without";
import { useTranslation } from "react-i18next";
import { Alert } from "react-bootstrap";

import {
  alertService,
  AlertInfoEx,
  kAlertHostId,
  AlertInfo,
} from "@/services/alert";
import { convertI18nText } from "@/common";

interface AutoCloseAlertProps {
  alert: AlertInfo;
  close: () => void;
}

export const AutoCloseAlert: React.FC<AutoCloseAlertProps> = (props) => {
  const { alert, close } = props;
  const { dismissTime } = alert;

  const { t } = useTranslation();

  const timerTag = React.useRef<number | null>(null);
  const closeHandler = React.useRef<(() => void) | null>(null);

  React.useEffect(() => {
    closeHandler.current = close;
  }, [close]);

  React.useEffect(() => {
    const tag =
      dismissTime === "never"
        ? null
        : typeof dismissTime === "number"
        ? window.setTimeout(() => closeHandler.current?.(), dismissTime)
        : window.setTimeout(() => closeHandler.current?.(), 5000);
    timerTag.current = tag;
    return () => {
      if (tag != null) {
        window.clearTimeout(tag);
      }
    };
  }, [dismissTime]);

  const cancelTimer = (): void => {
    const { current: tag } = timerTag;
    if (tag != null) {
      window.clearTimeout(tag);
    }
  };

  return (
    <Alert
      className="m-3"
      variant={alert.type ?? "primary"}
      onClick={cancelTimer}
      onClose={close}
      dismissible
    >
      {(() => {
        const { message } = alert;
        if (typeof message === "function") {
          const Message = message;
          return <Message />;
        } else return convertI18nText(message, t);
      })()}
    </Alert>
  );
};

const AlertHost: React.FC = () => {
  const [alerts, setAlerts] = React.useState<AlertInfoEx[]>([]);

  // react guarantee that state setters are stable, so we don't need to add it to dependency list

  React.useEffect(() => {
    const consume = (alert: AlertInfoEx): void => {
      setAlerts((old) => [...old, alert]);
    };

    alertService.registerConsumer(consume);
    return () => {
      alertService.unregisterConsumer(consume);
    };
  }, []);

  return (
    <div id={kAlertHostId} className="alert-container">
      {alerts.map((alert) => {
        return (
          <AutoCloseAlert
            key={alert.id}
            alert={alert}
            close={() => {
              setAlerts((old) => without(old, alert));
            }}
          />
        );
      })}
    </div>
  );
};

export default AlertHost;
