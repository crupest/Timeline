import i18n from "i18next";
import { useTranslation } from "react-i18next";

export type I18nText =
  | string
  | { type: "text" | "custom"; value: string }
  | { type: "i18n"; value: string };

type T = typeof i18n.t;

export function convertI18nText(text: I18nText, t: T): string {
  if (typeof text === "string") {
    return t(text);
  } else if (text.type === "i18n") {
    return t(text.value);
  } else {
    return text.value;
  }
}

export interface C {
  (text: I18nText): string;
}

export function createC(t: T): C {
  return ((text) => convertI18nText(text, t)) as C;
}

export const c = createC(i18n.t);

export function useC(ns?: string): C {
  const { t } = useTranslation(ns);
  return createC(t);
}

