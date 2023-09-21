import { ThemeColor, I18nText } from "../common";

const defaultDismissTime = 5000;

export interface AlertInfo {
  color?: ThemeColor;
  message: I18nText;
  dismissTime?: number | "never";
}

export interface AlertInfoWithId extends AlertInfo {
  id: number;
}

interface AlertServiceAlert extends AlertInfoWithId {
  timerId: number | null;
}

export type AlertsListener = (alerts: AlertInfoWithId[]) => void;

export class AlertService {
  private listeners: AlertsListener[] = [];
  private alerts: AlertServiceAlert[] = [];
  private currentId = 1;

  getAlert(alertId?: number | null | undefined): AlertServiceAlert | null {
    for (const alert of this.alerts) {
      if (alert.id === alertId) return alert;
    }
    return null;
  }

  registerListener(listener: AlertsListener): void {
    this.listeners.push(listener);
    listener(this.alerts);
  }

  unregisterListener(listener: AlertsListener): void {
    this.listeners = this.listeners.filter((l) => l !== listener);
  }

  notify() {
    for (const listener of this.listeners) {
      listener(this.alerts);
    }
  }

  push(alert: AlertInfo): void {
    const newAlert: AlertServiceAlert = {
      ...alert,
      id: this.currentId++,
      timerId: null,
    };

    this.alerts = [...this.alerts, newAlert];
    this._resetDismissTimer(newAlert);

    this.notify();
  }

  private _dismiss(alert: AlertServiceAlert) {
    if (alert.timerId != null) {
      window.clearTimeout(alert.timerId);
    }
    this.alerts = this.alerts.filter((a) => a !== alert);
    this.notify();
  }

  dismiss(alertId?: number | null | undefined) {
    const alert = this.getAlert(alertId);
    if (alert != null) {
      this._dismiss(alert);
    }
  }

  private _clearDismissTimer(alert: AlertServiceAlert) {
    if (alert.timerId != null) {
      window.clearTimeout(alert.timerId);
      alert.timerId = null;
    }
  }

  clearDismissTimer(alertId?: number | null | undefined) {
    const alert = this.getAlert(alertId);
    if (alert != null) {
      this._clearDismissTimer(alert);
    }
  }

  private _resetDismissTimer(
    alert: AlertServiceAlert,
    dismissTime?: number | null | undefined,
  ) {
    this._clearDismissTimer(alert);

    const realDismissTime =
      dismissTime ?? alert.dismissTime ?? defaultDismissTime;

    if (typeof realDismissTime === "number") {
      alert.timerId = window.setTimeout(() => {
        this._dismiss(alert);
      }, realDismissTime);
    }
  }

  resetDismissTimer(alertId?: number | null | undefined) {
    const alert = this.getAlert(alertId);
    if (alert != null) {
      this._resetDismissTimer(alert);
    }
  }
}

export const alertService = new AlertService();
