import React from "react";
import without from "lodash/without";
import { useTranslation } from "react-i18next";
import classNames from "classnames";

import { alertService, AlertInfoEx, AlertInfo } from "@/services/alert";
import { convertI18nText } from "@/common";

import "./alert.css";

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
    <div
      className={classNames(
        "m-3 cru-alert",
        "cru-" + (alert.type ?? "primary")
      )}
      onClick={cancelTimer}
    >
      <div className="cru-alert-content">
        {(() => {
          const { message, customMessage } = alert;
          if (customMessage != null) {
            return customMessage;
          } else {
            return convertI18nText(message, t);
          }
        })()}
      </div>
      <div className="cru-alert-close-button-container">
        <i
          className={classNames("icon-button bi-x cru-alert-close-button")}
          onClick={close}
        />
      </div>
    </div>
  );
};

const AlertHost: React.FC = () => {
  const [alerts, setAlerts] = React.useState<AlertInfoEx[]>([]);

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
    <div className="alert-container">
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
