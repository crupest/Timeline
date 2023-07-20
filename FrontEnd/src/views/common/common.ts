export type { Text, I18nText } from "@/common";
export { c, convertI18nText, useC } from "@/common";

export const themeColors = [
  "primary",
  "secondary",
  "tertiary",
  "danger",
  "success",
] as const;

export type ThemeColor = (typeof themeColors)[number];

export { breakpoints } from "./breakpoints";
export { useMobile } from "./hooks";
