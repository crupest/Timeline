// This error is thrown when ui goes wrong with bad logic.
// Such as a variable should not be null, but it does.
// This error should never occur. If it does, it indicates there is some logic bug in codes.
export class UiLogicError extends Error {}

export type { I18nText } from "./i18n";
export type { I18nText as Text } from "./i18n";
export { c, convertI18nText } from "./i18n";
export { default as useC } from "./utilities/hooks/use-c";
