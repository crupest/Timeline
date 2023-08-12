export type { Text, I18nText } from "@/common";
export { c, convertI18nText, useC } from "@/common";

export const themeColors = [
  "primary",
  "secondary",
  "danger",
  "create",
] as const;

export type ThemeColor = (typeof themeColors)[number];

export { breakpoints } from "./breakpoints";
export { useMobile } from "./hooks";
