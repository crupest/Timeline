import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { TwitterPicker } from "react-color";
import moment from "moment";

import { convertI18nText, I18nText, UiLogicError } from "@/common";

import { PaletteColorType } from "@/palette";

import Button from "../button/Button";
import LoadingButton from "../button/LoadingButton";
import Dialog from "./Dialog";

import "./OperationDialog.css";

interface DefaultErrorPromptProps {
  error?: string;
}

const DefaultErrorPrompt: React.FC<DefaultErrorPromptProps> = (props) => {
  const { t } = useTranslation();

  let result = <p className="cru-color-danger">{t("operationDialog.error")}</p>;

  if (props.error != null) {
    result = (
      <>
        {result}
        <p className="cru-color-danger">{props.error}</p>
      </>
    );
  }

  return result;
};

export interface OperationDialogTextInput {
  type: "text";
  label?: I18nText;
  password?: boolean;
  initValue?: string;
  textFieldProps?: Omit<
    React.InputHTMLAttributes<HTMLInputElement>,
    "type" | "value" | "onChange" | "aria-relevant"
  >;
  helperText?: string;
}

export interface OperationDialogBoolInput {
  type: "bool";
  label: I18nText;
  initValue?: boolean;
}

export interface OperationDialogSelectInputOption {
  value: string;
  label: I18nText;
  icon?: React.ReactElement;
}

export interface OperationDialogSelectInput {
  type: "select";
  label: I18nText;
  options: OperationDialogSelectInputOption[];
  initValue?: string;
}

export interface OperationDialogColorInput {
  type: "color";
  label?: I18nText;
  initValue?: string | null;
  canBeNull?: boolean;
}

export interface OperationDialogDateTimeInput {
  type: "datetime";
  label?: I18nText;
  initValue?: string;
}

export type OperationDialogInput =
  | OperationDialogTextInput
  | OperationDialogBoolInput
  | OperationDialogSelectInput
  | OperationDialogColorInput
  | OperationDialogDateTimeInput;

interface OperationInputTypeStringToValueTypeMap {
  text: string;
  bool: boolean;
  select: string;
  color: string | null;
  datetime: string;
}

type MapOperationInputTypeStringToValueType<Type> =
  Type extends keyof OperationInputTypeStringToValueTypeMap
    ? OperationInputTypeStringToValueTypeMap[Type]
    : never;

type MapOperationInputInfoValueType<T> = T extends OperationDialogInput
  ? MapOperationInputTypeStringToValueType<T["type"]>
  : T;

const initValueMapperMap: {
  [T in OperationDialogInput as T["type"]]: (
    item: T
  ) => MapOperationInputInfoValueType<T>;
} = {
  bool: (item) => item.initValue ?? false,
  color: (item) => item.initValue ?? null,
  datetime: (item) => {
    if (item.initValue != null) {
      return moment(item.initValue).format("YYYY-MM-DDTHH:mm:ss");
    } else {
      return "";
    }
  },
  select: (item) => item.initValue ?? item.options[0].value,
  text: (item) => item.initValue ?? "",
};

type MapOperationInputInfoValueTypeList<
  Tuple extends readonly OperationDialogInput[]
> = {
  [Index in keyof Tuple]: MapOperationInputInfoValueType<Tuple[Index]>;
} & { length: Tuple["length"] };

export type OperationInputError =
  | {
      [index: number]: I18nText | null | undefined;
    }
  | null
  | undefined;

const isNoError = (error: OperationInputError): boolean => {
  if (error == null) return true;
  for (const key in error) {
    if (error[key] != null) return false;
  }
  return true;
};

export interface OperationDialogProps<
  TData,
  OperationInputInfoList extends readonly OperationDialogInput[]
> {
  open: boolean;
  onClose: () => void;
  title: I18nText | (() => React.ReactNode);
  themeColor?: PaletteColorType;
  onProcess: (
    inputs: MapOperationInputInfoValueTypeList<OperationInputInfoList>
  ) => Promise<TData>;
  inputScheme?: OperationInputInfoList;
  inputValidator?: (
    inputs: MapOperationInputInfoValueTypeList<OperationInputInfoList>
  ) => OperationInputError;
  inputPrompt?: I18nText | (() => React.ReactNode);
  processPrompt?: () => React.ReactNode;
  successPrompt?: (data: TData) => React.ReactNode;
  failurePrompt?: (error: unknown) => React.ReactNode;
  onSuccessAndClose?: (data: TData) => void;
}

const OperationDialog = <
  TData,
  OperationInputInfoList extends readonly OperationDialogInput[]
>(
  props: OperationDialogProps<TData, OperationInputInfoList>
): React.ReactElement => {
  const inputScheme = (props.inputScheme ??
    []) as readonly OperationDialogInput[];

  const { t } = useTranslation();

  type Step =
    | { type: "input" }
    | { type: "process" }
    | {
        type: "success";
        data: TData;
      }
    | {
        type: "failure";
        data: unknown;
      };
  const [step, setStep] = useState<Step>({ type: "input" });

  type ValueType = boolean | string | null | undefined;

  const [values, setValues] = useState<ValueType[]>(
    inputScheme.map((item) => {
      if (item.type in initValueMapperMap) {
        return (
          initValueMapperMap[item.type] as (
            i: OperationDialogInput
          ) => ValueType
        )(item);
      } else {
        throw new UiLogicError("Unknown input scheme.");
      }
    })
  );
  const [dirtyList, setDirtyList] = useState<boolean[]>(() =>
    inputScheme.map(() => false)
  );
  const [inputError, setInputError] = useState<OperationInputError>();

  const close = (): void => {
    if (step.type !== "process") {
      props.onClose();
      if (step.type === "success" && props.onSuccessAndClose) {
        props.onSuccessAndClose(step.data);
      }
    } else {
      console.log("Attempt to close modal when processing.");
    }
  };

  const onConfirm = (): void => {
    setStep({ type: "process" });
    props
      .onProcess(
        values.map((v, index) => {
          if (inputScheme[index].type === "datetime" && v !== "")
            return new Date(v as string).toISOString();
          else return v;
        }) as unknown as MapOperationInputInfoValueTypeList<OperationInputInfoList>
      )
      .then(
        (d) => {
          setStep({
            type: "success",
            data: d,
          });
        },
        (e: unknown) => {
          setStep({
            type: "failure",
            data: e,
          });
        }
      );
  };

  let body: React.ReactNode;
  if (step.type === "input" || step.type === "process") {
    const process = step.type === "process";

    let inputPrompt =
      typeof props.inputPrompt === "function"
        ? props.inputPrompt()
        : convertI18nText(props.inputPrompt, t);
    inputPrompt = <h6>{inputPrompt}</h6>;

    const validate = (values: ValueType[]): boolean => {
      const { inputValidator } = props;
      if (inputValidator != null) {
        const result = inputValidator(
          values as unknown as MapOperationInputInfoValueTypeList<OperationInputInfoList>
        );
        setInputError(result);
        return isNoError(result);
      }
      return true;
    };

    const updateValue = (index: number, newValue: ValueType): void => {
      const oldValues = values;
      const newValues = oldValues.slice();
      newValues[index] = newValue;
      setValues(newValues);
      if (dirtyList[index] === false) {
        const newDirtyList = dirtyList.slice();
        newDirtyList[index] = true;
        setDirtyList(newDirtyList);
      }
      validate(newValues);
    };

    const canProcess = isNoError(inputError);

    body = (
      <>
        <div>
          {inputPrompt}
          {inputScheme.map((item, index) => {
            const value = values[index];
            const error: string | null =
              dirtyList[index] && inputError != null
                ? convertI18nText(inputError[index], t)
                : null;

            if (item.type === "text") {
              return (
                <div key={index}>
                  {item.label && (
                    <label>{convertI18nText(item.label, t)}</label>
                  )}
                  <input
                    type={item.password === true ? "password" : "text"}
                    value={value as string}
                    onChange={(e) => {
                      const v = e.target.value;
                      updateValue(index, v);
                    }}
                    disabled={process}
                  />
                  {error != null && <div>{error}</div>}
                  {item.helperText && <div>{t(item.helperText)}</div>}
                </div>
              );
            } else if (item.type === "bool") {
              return (
                <div key={index}>
                  <input
                    type="checkbox"
                    checked={value as boolean}
                    onChange={(event) => {
                      updateValue(index, event.currentTarget.checked);
                    }}
                    disabled={process}
                  />
                  <label>{convertI18nText(item.label, t)}</label>
                </div>
              );
            } else if (item.type === "select") {
              return (
                <div key={index}>
                  <label>{convertI18nText(item.label, t)}</label>
                  <select
                    value={value as string}
                    onChange={(event) => {
                      updateValue(index, event.target.value);
                    }}
                    disabled={process}
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
                <div key={index}>
                  {item.canBeNull ? (
                    <input
                      type="checkbox"
                      checked={value !== null}
                      onChange={(event) => {
                        if (event.currentTarget.checked) {
                          updateValue(index, "#007bff");
                        } else {
                          updateValue(index, null);
                        }
                      }}
                      disabled={process}
                    />
                  ) : null}
                  <label>{convertI18nText(item.label, t)}</label>
                  {value !== null && (
                    <TwitterPicker
                      color={value as string}
                      onChange={(result) => updateValue(index, result.hex)}
                    />
                  )}
                </div>
              );
            } else if (item.type === "datetime") {
              return (
                <div key={index}>
                  {item.label && (
                    <label>{convertI18nText(item.label, t)}</label>
                  )}
                  <input
                    type="datetime-local"
                    value={value as string}
                    onChange={(e) => {
                      const v = e.target.value;
                      updateValue(index, v);
                    }}
                    disabled={process}
                  />
                  {error != null && <div>{error}</div>}
                </div>
              );
            }
          })}
        </div>
        <hr />
        <div className="cru-dialog-bottom-area">
          <Button
            text="operationDialog.cancel"
            color="secondary"
            outline
            onClick={close}
          />
          <LoadingButton
            color={props.themeColor}
            loading={process}
            disabled={!canProcess}
            onClick={() => {
              setDirtyList(inputScheme.map(() => true));
              if (validate(values)) {
                onConfirm();
              }
            }}
          >
            {t("operationDialog.confirm")}
          </LoadingButton>
        </div>
      </>
    );
  } else {
    let content: React.ReactNode;
    const result = step;
    if (result.type === "success") {
      content =
        props.successPrompt?.(result.data) ?? t("operationDialog.success");
      if (typeof content === "string")
        content = <p className="cru-color-success">{content}</p>;
    } else {
      content = props.failurePrompt?.(result.data) ?? <DefaultErrorPrompt />;
      if (typeof content === "string")
        content = <DefaultErrorPrompt error={content} />;
    }
    body = (
      <>
        <div>{content}</div>
        <hr />
        <div>
          <Button text="operationDialog.ok" color="primary" onClick={close} />
        </div>
      </>
    );
  }

  const title =
    typeof props.title === "function"
      ? props.title()
      : convertI18nText(props.title, t);

  return (
    <Dialog open={props.open} onClose={close}>
      <h3
        className={
          props.themeColor != null ? "cru-color-" + props.themeColor : undefined
        }
      >
        {title}
      </h3>
      <hr />
      {body}
    </Dialog>
  );
};

export default OperationDialog;
