import React from "react";
import pull from "lodash/pull";

import { I18nText } from "@/common";
import { PaletteColorType } from "@/palette";

export interface AlertInfo {
  type?: PaletteColorType;
  message?: I18nText;
  customMessage?: React.ReactElement;
  dismissTime?: number | "never";
}

export interface AlertInfoEx extends AlertInfo {
  id: number;
}

export type AlertConsumer = (alerts: AlertInfoEx) => void;

export class AlertService {
  private consumers: AlertConsumer[] = [];
  private savedAlerts: AlertInfoEx[] = [];
  private currentId = 1;

  private produce(alert: AlertInfoEx): void {
    for (const consumer of this.consumers) {
      consumer(alert);
    }
  }

  registerConsumer(consumer: AlertConsumer): void {
    this.consumers.push(consumer);
    if (this.savedAlerts.length !== 0) {
      for (const alert of this.savedAlerts) {
        this.produce(alert);
      }
      this.savedAlerts = [];
    }
  }

  unregisterConsumer(consumer: AlertConsumer): void {
    pull(this.consumers, consumer);
  }

  push(alert: AlertInfo): void {
    const newAlert: AlertInfoEx = { ...alert, id: this.currentId++ };
    if (this.consumers.length === 0) {
      this.savedAlerts.push(newAlert);
    } else {
      this.produce(newAlert);
    }
  }
}

export const alertService = new AlertService();

export function pushAlert(alert: AlertInfo): void {
  alertService.push(alert);
}
