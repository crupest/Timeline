import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { Form, Button, Modal } from "react-bootstrap";
import { ChromePicker } from "react-color";

import { convertI18nText, I18nText, UiLogicError } from "@/common";

import LoadingButton from "./LoadingButton";

interface DefaultErrorPromptProps {
  error?: string;
}

const DefaultErrorPrompt: React.FC<DefaultErrorPromptProps> = (props) => {
  const { t } = useTranslation();

  let result = <p className="text-danger">{t("operationDialog.error")}</p>;

  if (props.error != null) {
    result = (
      <>
        {result}
        <p className="text-danger">{props.error}</p>
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
  disableAlpha?: boolean;
  canBeNull?: boolean;
}

export type OperationDialogInput =
  | OperationDialogTextInput
  | OperationDialogBoolInput
  | OperationDialogSelectInput
  | OperationDialogColorInput;

type MapOperationInputInfoValueType<T> = T extends OperationDialogTextInput
  ? string
  : T extends OperationDialogBoolInput
  ? boolean
  : T extends OperationDialogSelectInput
  ? string
  : T extends OperationDialogColorInput
  ? string | null
  : never;

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
  close: () => void;
  title: I18nText | (() => React.ReactNode);
  themeColor?: "danger" | "success" | string;
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
    inputScheme.map((i) => {
      if (i.type === "bool") {
        return i.initValue ?? false;
      } else if (i.type === "text" || i.type === "select") {
        return i.initValue ?? "";
      } else if (i.type === "color") {
        return i.initValue ?? null;
      }
      {
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
      props.close();
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
        (values as unknown) as MapOperationInputInfoValueTypeList<OperationInputInfoList>
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
          (values as unknown) as MapOperationInputInfoValueTypeList<OperationInputInfoList>
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
        <Modal.Body>
          {inputPrompt}
          {inputScheme.map((item, index) => {
            const value = values[index];
            const error: string | null =
              dirtyList[index] && inputError != null
                ? convertI18nText(inputError[index], t)
                : null;

            if (item.type === "text") {
              return (
                <Form.Group key={index}>
                  {item.label && (
                    <Form.Label>{convertI18nText(item.label, t)}</Form.Label>
                  )}
                  <Form.Control
                    type={item.password === true ? "password" : "text"}
                    value={value as string}
                    onChange={(e) => {
                      const v = e.target.value;
                      updateValue(index, v);
                    }}
                    isInvalid={error != null}
                    disabled={process}
                  />
                  {error != null && (
                    <Form.Control.Feedback type="invalid">
                      {error}
                    </Form.Control.Feedback>
                  )}
                  {item.helperText && (
                    <Form.Text>{t(item.helperText)}</Form.Text>
                  )}
                </Form.Group>
              );
            } else if (item.type === "bool") {
              return (
                <Form.Group key={index}>
                  <Form.Check<"input">
                    type="checkbox"
                    checked={value as boolean}
                    onChange={(event) => {
                      updateValue(index, event.currentTarget.checked);
                    }}
                    label={convertI18nText(item.label, t)}
                    disabled={process}
                  />
                </Form.Group>
              );
            } else if (item.type === "select") {
              return (
                <Form.Group key={index}>
                  <Form.Label>{convertI18nText(item.label, t)}</Form.Label>
                  <Form.Control
                    as="select"
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
                  </Form.Control>
                </Form.Group>
              );
            } else if (item.type === "color") {
              return (
                <Form.Group key={index}>
                  {item.canBeNull ? (
                    <Form.Check<"input">
                      type="checkbox"
                      checked={value !== null}
                      onChange={(event) => {
                        if (event.currentTarget.checked) {
                          updateValue(index, "#007bff");
                        } else {
                          updateValue(index, null);
                        }
                      }}
                      label={convertI18nText(item.label, t)}
                      disabled={process}
                    />
                  ) : (
                    <Form.Label>{convertI18nText(item.label, t)}</Form.Label>
                  )}
                  {value !== null && (
                    <ChromePicker
                      color={value as string}
                      onChange={(result) => updateValue(index, result.hex)}
                      disableAlpha={item.disableAlpha}
                    />
                  )}
                </Form.Group>
              );
            }
          })}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={close}>
            {t("operationDialog.cancel")}
          </Button>
          <LoadingButton
            variant={props.themeColor}
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
        </Modal.Footer>
      </>
    );
  } else {
    let content: React.ReactNode;
    const result = step;
    if (result.type === "success") {
      content =
        props.successPrompt?.(result.data) ?? t("operationDialog.success");
      if (typeof content === "string")
        content = <p className="text-success">{content}</p>;
    } else {
      content = props.failurePrompt?.(result.data) ?? <DefaultErrorPrompt />;
      if (typeof content === "string")
        content = <DefaultErrorPrompt error={content} />;
    }
    body = (
      <>
        <Modal.Body>{content}</Modal.Body>
        <Modal.Footer>
          <Button variant="primary" onClick={close}>
            {t("operationDialog.ok")}
          </Button>
        </Modal.Footer>
      </>
    );
  }

  const title =
    typeof props.title === "function"
      ? props.title()
      : convertI18nText(props.title, t);

  return (
    <Modal show={props.open} onHide={close}>
      <Modal.Header
        className={
          props.themeColor != null ? "text-" + props.themeColor : undefined
        }
      >
        {title}
      </Modal.Header>
      {body}
    </Modal>
  );
};

export default OperationDialog;
