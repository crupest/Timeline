import { useState, ReactNode, ComponentPropsWithoutRef } from "react";
import classNames from "classnames";
import moment from "moment";

import { useC, Text, ThemeColor } from "../common";

import Button from "../button/Button";
import LoadingButton from "../button/LoadingButton";
import Dialog from "./Dialog";

import "./OperationDialog.css";

interface DefaultPromptProps {
  color?: ThemeColor;
  message?: Text;
  customMessage?: ReactNode;
  className?: string;
}

function DefaultPrompt(props: DefaultPromptProps) {
  const { color, message, customMessage, className } = props;

  const c = useC();

  return (
    <div className={classNames(className, `cru-${color ?? "primary"}`)}>
      <p>{c(message)}</p>
      {customMessage}
    </div>
  );
}

export interface OperationDialogTextInput {
  type: "text";
  label?: Text;
  password?: boolean;
  initValue?: string;
  textFieldProps?: Omit<
    ComponentPropsWithoutRef<"input">,
    "type" | "value" | "onChange"
  >;
  helperText?: Text;
}

export interface OperationDialogBoolInput {
  type: "bool";
  label: Text;
  initValue?: boolean;
  helperText?: Text;
}

export interface OperationDialogSelectInputOption {
  value: string;
  label: Text;
  icon?: ReactNode;
}

export interface OperationDialogSelectInput {
  type: "select";
  label: Text;
  options: OperationDialogSelectInputOption[];
  initValue?: string;
}

export interface OperationDialogDateTimeInput {
  type: "datetime";
  label?: Text;
  initValue?: string;
  helperText?: string;
}

export type OperationDialogInput =
  | OperationDialogTextInput
  | OperationDialogBoolInput
  | OperationDialogSelectInput
  | OperationDialogDateTimeInput;

interface OperationInputTypeStringToValueTypeMap {
  text: string;
  bool: boolean;
  select: string;
  datetime: string;
}

type OperationInputValueType =
  OperationInputTypeStringToValueTypeMap[keyof OperationInputTypeStringToValueTypeMap];

type MapOperationInputTypeStringToValueType<Type> =
  Type extends keyof OperationInputTypeStringToValueTypeMap
    ? OperationInputTypeStringToValueTypeMap[Type]
    : never;

type MapOperationInputInfoValueType<T> = T extends OperationDialogInput
  ? MapOperationInputTypeStringToValueType<T["type"]>
  : T;

type MapOperationInputInfoValueTypeList<
  Tuple extends readonly OperationDialogInput[],
> = {
  [Index in keyof Tuple]: MapOperationInputInfoValueType<Tuple[Index]>;
};

export type OperationInputError =
  | {
      [index: number]: Text | null | undefined;
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

type ItemValueMapper = {
  [T in OperationDialogInput as T["type"]]: (
    item: T,
  ) => MapOperationInputInfoValueType<T>;
};

type ValueValueMapper = {
  [T in OperationDialogInput as T["type"]]: (
    item: MapOperationInputInfoValueType<T>,
  ) => MapOperationInputInfoValueType<T>;
};

const initValueMapperMap: ItemValueMapper = {
  bool: (item) => item.initValue ?? false,
  datetime: (item) =>
    item.initValue != null
      ? /* cspell: disable-next-line */
        moment(item.initValue).format("YYYY-MM-DDTHH:mm:ss")
      : "",
  select: (item) => item.initValue ?? item.options[0].value,
  text: (item) => item.initValue ?? "",
};

const finalValueMapperMap: ValueValueMapper = {
  bool: (value) => value,
  datetime: (value) => new Date(value).toISOString(),
  select: (value) => value,
  text: (value) => value,
};

export interface OperationDialogProps<
  TData,
  OperationInputInfoList extends readonly OperationDialogInput[],
> {
  open: boolean;
  onClose: () => void;

  themeColor?: ThemeColor;
  title: Text;
  inputPrompt?: Text;
  processPrompt?: Text;
  successPrompt?: (data: TData) => ReactNode;
  failurePrompt?: (error: unknown) => ReactNode;

  inputScheme?: OperationInputInfoList;
  inputValidator?: (
    inputs: MapOperationInputInfoValueTypeList<OperationInputInfoList>,
  ) => OperationInputError;

  onProcess: (
    inputs: MapOperationInputInfoValueTypeList<OperationInputInfoList>,
  ) => Promise<TData>;
  onSuccessAndClose?: (data: TData) => void;
}

function OperationDialog<
  TData,
  OperationInputInfoList extends readonly OperationDialogInput[],
>(props: OperationDialogProps<TData, OperationInputInfoList>) {
  const inputScheme = props.inputScheme ?? ([] as const);

  const c = useC();

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

  type Values = MapOperationInputInfoValueTypeList<OperationInputInfoList>;

  const [values, setValues] = useState<Values>(
    () =>
      inputScheme.map((item) =>
        initValueMapperMap[item.type](item as never),
      ) as Values,
  );

  const [dirtyList, setDirtyList] = useState<boolean[]>(() =>
    inputScheme.map(() => false),
  );

  const [inputError, setInputError] = useState<OperationInputError>();

  function close() {
    if (step.type !== "process") {
      props.onClose();
      if (step.type === "success" && props.onSuccessAndClose) {
        props.onSuccessAndClose(step.data);
      }
    } else {
      console.log("Attempt to close modal dialog when processing.");
    }
  }

  function onConfirm() {
    setStep({ type: "process" });
    props
      .onProcess(
        values.map((value, index) =>
          finalValueMapperMap[inputScheme[index].type](value as never),
        ) as Values,
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
        },
      );
  }

  let body: ReactNode;
  if (step.type === "input" || step.type === "process") {
    const process = step.type === "process";

    const validate = (values: Values): boolean => {
      const { inputValidator } = props;
      if (inputValidator != null) {
        const result = inputValidator(values);
        setInputError(result);
        return isNoError(result);
      }
      return true;
    };

    const updateValue = (
      index: number,
      newValue: OperationInputValueType,
    ): void => {
      const oldValues = values;
      const newValues = oldValues.slice();
      newValues[index] = newValue;
      setValues(newValues as Values);
      if (dirtyList[index] === false) {
        const newDirtyList = dirtyList.slice();
        newDirtyList[index] = true;
        setDirtyList(newDirtyList);
      }
      validate(newValues as Values);
    };

    const canProcess = isNoError(inputError);

    body = (
      <div className="cru-operation-dialog-main-area">
        <div>
          <div>{c(props.inputPrompt)}</div>
          {inputScheme.map((item: OperationDialogInput, index: number) => {
            const value = values[index];
            const error: string | null =
              dirtyList[index] && inputError != null
                ? c(inputError[index])
                : null;

            if (item.type === "text") {
              return (
                <div
                  key={index}
                  className={classNames(
                    "cru-operation-dialog-group",
                    error && "error",
                  )}
                >
                  {item.label && (
                    <label className="cru-operation-dialog-label">
                      {c(item.label)}
                    </label>
                  )}
                  <input
                    type={item.password === true ? "password" : "text"}
                    value={value as string}
                    onChange={(event) => {
                      const v = event.target.value;
                      updateValue(index, v);
                    }}
                    disabled={process}
                  />
                  {error && (
                    <div className="cru-operation-dialog-error-text">
                      {error}
                    </div>
                  )}
                  {item.helperText && (
                    <div className="cru-operation-dialog-helper-text">
                      {c(item.helperText)}
                    </div>
                  )}
                </div>
              );
            } else if (item.type === "bool") {
              return (
                <div
                  key={index}
                  className={classNames(
                    "cru-operation-dialog-group",
                    error && "error",
                  )}
                >
                  <input
                    type="checkbox"
                    checked={value as boolean}
                    onChange={(event) => {
                      const v = event.currentTarget.checked;
                      updateValue(index, v);
                    }}
                    disabled={process}
                  />
                  <label className="cru-operation-dialog-inline-label">
                    {c(item.label)}
                  </label>
                  {error && (
                    <div className="cru-operation-dialog-error-text">
                      {error}
                    </div>
                  )}
                  {item.helperText && (
                    <div className="cru-operation-dialog-helper-text">
                      {c(item.helperText)}
                    </div>
                  )}
                </div>
              );
            } else if (item.type === "select") {
              return (
                <div
                  key={index}
                  className={classNames(
                    "cru-operation-dialog-group",
                    error && "error",
                  )}
                >
                  <label className="cru-operation-dialog-label">
                    {c(item.label)}
                  </label>
                  <select
                    value={value as string}
                    onChange={(event) => {
                      const e = event.target.value;
                      updateValue(index, e);
                    }}
                    disabled={process}
                  >
                    {item.options.map((option, i) => {
                      return (
                        <option value={option.value} key={i}>
                          {option.icon}
                          {c(option.label)}
                        </option>
                      );
                    })}
                  </select>
                </div>
              );
            } else if (item.type === "datetime") {
              return (
                <div
                  key={index}
                  className={classNames(
                    "cru-operation-dialog-group",
                    error && "error",
                  )}
                >
                  {item.label && (
                    <label className="cru-operation-dialog-label">
                      {c(item.label)}
                    </label>
                  )}
                  <input
                    type="datetime-local"
                    value={value as string}
                    onChange={(event) => {
                      const v = event.target.value;
                      updateValue(index, v);
                    }}
                    disabled={process}
                  />
                  {error && (
                    <div className="cru-operation-dialog-error-text">
                      {error}
                    </div>
                  )}
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
            disabled={process}
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
            {c("operationDialog.confirm")}
          </LoadingButton>
        </div>
      </div>
    );
  } else {
    const result = step;

    const promptProps: DefaultPromptProps =
      result.type === "success"
        ? {
            color: "success",
            message: "operationDialog.success",
            customMessage: props.successPrompt?.(result.data),
          }
        : {
            color: "danger",
            message: "operationDialog.error",
            customMessage: props.failurePrompt?.(result.data),
          };
    body = (
      <div className="cru-operation-dialog-main-area">
        <DefaultPrompt {...promptProps} />
        <hr />
        <div className="cru-dialog-bottom-area">
          <Button text="operationDialog.ok" color="primary" onClick={close} />
        </div>
      </div>
    );
  }

  return (
    <Dialog open={props.open} onClose={close}>
      <div
        className={`cru-operation-dialog-title cru-${
          props.themeColor ?? "primary"
        }`}
      >
        {c(props.title)}
      </div>
      <hr />
      {body}
    </Dialog>
  );
}

export default OperationDialog;
