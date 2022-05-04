import React from "react";
import classNames from "classnames";
import { useTranslation } from "react-i18next";
import { TwitterPicker } from "react-color";

import { convertI18nText, I18nText } from "@/common";

import "./InputPanel.css";

export interface TextInput {
  type: "text";
  label?: I18nText;
  helper?: I18nText;
  password?: boolean;
}

export interface BoolInput {
  type: "bool";
  label: I18nText;
  helper?: I18nText;
}

export interface SelectInputOption {
  value: string;
  label: I18nText;
  icon?: React.ReactElement;
}

export interface SelectInput {
  type: "select";
  label: I18nText;
  options: SelectInputOption[];
}

export interface ColorInput {
  type: "color";
  label?: I18nText;
}

export interface DateTimeInput {
  type: "datetime";
  label?: I18nText;
  helper?: I18nText;
}

export type Input =
  | TextInput
  | BoolInput
  | SelectInput
  | ColorInput
  | DateTimeInput;

interface InputTypeToValueTypeMap {
  text: string;
  bool: boolean;
  select: string;
  color: string;
  datetime: string;
}

type ValueTypes = InputTypeToValueTypeMap[keyof InputTypeToValueTypeMap];

type MapInputTypeToValueType<Type> = Type extends keyof InputTypeToValueTypeMap
  ? InputTypeToValueTypeMap[Type]
  : never;

type MapInputToValueType<T> = T extends Input
  ? MapInputTypeToValueType<T["type"]>
  : T;

type MapInputListToValueTypeList<Tuple extends readonly Input[]> = {
  [Index in keyof Tuple]: MapInputToValueType<Tuple[Index]>;
} & { length: Tuple["length"] };

export type InputPanelError = {
  [index: number]: I18nText | null | undefined;
};

export function hasError(e: InputPanelError | null | undefined): boolean {
  if (e == null) return false;
  for (const key of Object.keys(e)) {
    if (e[key as unknown as number] != null) return true;
  }
  return false;
}

export interface InputPanelProps<InputList extends readonly Input[]> {
  scheme: InputList;
  values: MapInputListToValueTypeList<InputList>;
  onChange: (
    values: MapInputListToValueTypeList<InputList>,
    index: number
  ) => void;
  error?: InputPanelError;
  disable?: boolean;
}

const InputPanel = <InputList extends readonly Input[]>(
  props: InputPanelProps<InputList>
): React.ReactElement => {
  const { values, onChange, scheme, error, disable } = props;

  const { t } = useTranslation();

  const updateValue = (index: number, newValue: ValueTypes): void => {
    const oldValues = values;
    const newValues = oldValues.slice();
    newValues[index] = newValue;
    onChange(
      newValues as unknown as MapInputListToValueTypeList<InputList>,
      index
    );
  };

  return (
    <div>
      {scheme.map((item, index) => {
        const v = values[index];
        const e: string | null = convertI18nText(error?.[index], t);

        if (item.type === "text") {
          return (
            <div
              key={index}
              className={classNames("cru-input-panel-group", e && "error")}
            >
              {item.label && (
                <label className="cru-input-panel-label">
                  {convertI18nText(item.label, t)}
                </label>
              )}
              <input
                type={item.password === true ? "password" : "text"}
                value={v as string}
                onChange={(e) => {
                  const v = e.target.value;
                  updateValue(index, v);
                }}
                disabled={disable}
              />
              {e && <div className="cru-input-panel-error-text">{e}</div>}
              {item.helper && (
                <div className="cru-input-panel-helper-text">
                  {convertI18nText(item.helper, t)}
                </div>
              )}
            </div>
          );
        } else if (item.type === "bool") {
          return (
            <div
              key={index}
              className={classNames("cru-input-panel-group", e && "error")}
            >
              <input
                type="checkbox"
                checked={v as boolean}
                onChange={(event) => {
                  const value = event.currentTarget.checked;
                  updateValue(index, value);
                }}
                disabled={disable}
              />
              <label className="cru-input-panel-inline-label">
                {convertI18nText(item.label, t)}
              </label>
              {e != null && (
                <div className="cru-input-panel-error-text">{e}</div>
              )}
              {item.helper && (
                <div className="cru-input-panel-helper-text">
                  {convertI18nText(item.helper, t)}
                </div>
              )}
            </div>
          );
        } else if (item.type === "select") {
          return (
            <div
              key={index}
              className={classNames("cru-input-panel-group", e && "error")}
            >
              <label className="cru-input-panel-label">
                {convertI18nText(item.label, t)}
              </label>
              <select
                value={v as string}
                onChange={(event) => {
                  const value = event.target.value;
                  updateValue(index, value);
                }}
                disabled={disable}
              >
                {item.options.map((option, i) => {
                  return (
                    <option value={option.value} key={i}>
                      {option.icon}
                      {convertI18nText(option.label, t)}
                    </option>
                  );
                })}
              </select>
            </div>
          );
        } else if (item.type === "color") {
          return (
            <div
              key={index}
              className={classNames("cru-input-panel-group", e && "error")}
            >
              <label className="cru-input-panel-inline-label">
                {convertI18nText(item.label, t)}
              </label>
              <TwitterPicker
                color={v as string}
                triangle="hide"
                onChange={(result) => updateValue(index, result.hex)}
              />
            </div>
          );
        } else if (item.type === "datetime") {
          return (
            <div
              key={index}
              className={classNames("cru-input-panel-group", e && "error")}
            >
              {item.label && (
                <label className="cru-input-panel-label">
                  {convertI18nText(item.label, t)}
                </label>
              )}
              <input
                type="datetime-local"
                value={v as string}
                onChange={(e) => {
                  const v = e.target.value;
                  updateValue(index, v);
                }}
                disabled={disable}
              />
              {e != null && (
                <div className="cru-input-panel-error-text">{e}</div>
              )}
              {item.helper && (
                <div className="cru-input-panel-helper-text">
                  {convertI18nText(item.helper, t)}
                </div>
              )}
            </div>
          );
        }
      })}
    </div>
  );
};

export default InputPanel;
