import "./index.css";

export type { Text, I18nText } from "~src/common";
export { UiLogicError, c, convertI18nText, useC } from "~src/common";

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
