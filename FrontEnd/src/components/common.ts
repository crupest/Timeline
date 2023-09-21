import "./index.css";

export type { I18nText } from "~src/i18n";
export { convertI18nText, useC } from "~src/i18n";

export class UiLogicError extends Error {}

export const themeColors = [
  "primary",
  "secondary",
  "danger",
  "create",
] as const;

export type ThemeColor = (typeof themeColors)[number];

export type ClickableColor = ThemeColor | "grayscale" | "light" | "minor";

export { breakpoints } from "./breakpoints";

export * as geometry from "~src/utilities/geometry";

export * as array from "~src/utilities/array";
