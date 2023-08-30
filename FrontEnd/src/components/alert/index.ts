import { alertService, AlertInfo } from "./AlertService";
import { default as AlertHost } from "./AlertHost";

export { alertService, AlertHost };

export function pushAlert(alert: AlertInfo): void {
  alertService.push(alert);
}
