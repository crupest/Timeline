import React from "react";
import { Observable, Subject } from "rxjs";
import { TFunction } from "i18next";

// This error is thrown when ui goes wrong with bad logic.
// Such as a variable should not be null, but it does.
// This error should never occur. If it does, it indicates there is some logic bug in codes.
export class UiLogicError extends Error {}

export function useEventEmiiter(): [() => Observable<null>, () => void] {
  const ref = React.useRef<Subject<null> | null>(null);

  return React.useMemo(() => {
    const getter = (): Subject<null> => {
      if (ref.current == null) {
        ref.current = new Subject<null>();
      }
      return ref.current;
    };
    const trigger = (): void => {
      getter().next(null);
    };
    return [getter, trigger];
  }, []);
}

export function useValueEventEmiiter<T>(): [
  () => Observable<T>,
  (value: T) => void
] {
  const ref = React.useRef<Subject<T> | null>(null);

  return React.useMemo(() => {
    const getter = (): Subject<T> => {
      if (ref.current == null) {
        ref.current = new Subject<T>();
      }
      return ref.current;
    };
    const trigger = (value: T): void => {
      getter().next(value);
    };
    return [getter, trigger];
  }, []);
}

export type I18nText =
  | string
  | { type: "custom"; value: string }
  | { type: "i18n"; value: string };

export function convertI18nText(text: I18nText, t: TFunction): string;
export function convertI18nText(
  text: I18nText | null | undefined,
  t: TFunction
): string | null;
export function convertI18nText(
  text: I18nText | null | undefined,
  t: TFunction
): string | null {
  if (text == null) {
    return null;
  } else if (typeof text === "string") {
    return t(text);
  } else if (text.type === "i18n") {
    return t(text.value);
  } else {
    return text.value;
  }
}
