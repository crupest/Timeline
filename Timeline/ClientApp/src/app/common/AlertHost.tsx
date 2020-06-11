import React, { useCallback } from 'react';
import { Alert } from 'reactstrap';
import without from 'lodash/without';
import concat from 'lodash/concat';

import {
  alertService,
  AlertInfoEx,
  kAlertHostId,
  AlertInfo,
} from './alert-service';
import { useTranslation } from 'react-i18next';

interface AutoCloseAlertProps {
  alert: AlertInfo;
  close: () => void;
}

export const AutoCloseAlert: React.FC<AutoCloseAlertProps> = (props) => {
  const { alert } = props;
  const { dismissTime } = alert;

  const { t } = useTranslation();

  React.useEffect(() => {
    const tag =
      dismissTime === 'never'
        ? null
        : typeof dismissTime === 'number'
        ? window.setTimeout(props.close, dismissTime)
        : window.setTimeout(props.close, 5000);
    return () => {
      if (tag != null) {
        window.clearTimeout(tag);
      }
    };
  }, [dismissTime, props.close]);

  return (
    <Alert className="m-3" color={alert.type ?? 'primary'} toggle={props.close}>
      {(() => {
        const { message } = alert;
        if (typeof message === 'function') {
          const Message = message;
          return <Message />;
        } else if (typeof message === 'object' && message.type === 'i18n') {
          return t(message.key);
        } else return alert.message;
      })()}
    </Alert>
  );
};

// oh what a bad name!
interface AlertInfoExEx extends AlertInfoEx {
  close: () => void;
}

export const AlertHost: React.FC = () => {
  const [alerts, setAlerts] = React.useState<AlertInfoExEx[]>([]);

  // react guarantee that state setters are stable, so we don't need to add it to dependency list

  const consume = useCallback((alert: AlertInfoEx): void => {
    const alertEx: AlertInfoExEx = {
      ...alert,
      close: () => {
        setAlerts((oldAlerts) => {
          return without(oldAlerts, alertEx);
        });
      },
    };
    setAlerts((oldAlerts) => {
      return concat(oldAlerts, alertEx);
    });
  }, []);

  React.useEffect(() => {
    alertService.registerConsumer(consume);
    return () => {
      alertService.unregisterConsumer(consume);
    };
  }, [consume]);

  return (
    <div id={kAlertHostId} className="alert-container">
      {alerts.map((alert) => {
        return (
          <AutoCloseAlert key={alert.id} alert={alert} close={alert.close} />
        );
      })}
    </div>
  );
};

export default AlertHost;
