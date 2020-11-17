import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { Form, Button, Modal } from "react-bootstrap";

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

export interface OperationTextInputInfo {
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

export interface OperationBoolInputInfo {
  type: "bool";
  label: I18nText;
  initValue?: boolean;
}

export interface OperationSelectInputInfoOption {
  value: string;
  label: I18nText;
  icon?: React.ReactElement;
}

export interface OperationSelectInputInfo {
  type: "select";
  label: I18nText;
  options: OperationSelectInputInfoOption[];
  initValue?: string;
}

export type OperationInputInfo =
  | OperationTextInputInfo
  | OperationBoolInputInfo
  | OperationSelectInputInfo;

type MapOperationInputInfoValueType<T> = T extends OperationTextInputInfo
  ? string
  : T extends OperationBoolInputInfo
  ? boolean
  : T extends OperationSelectInputInfo
  ? string
  : never;

type MapOperationInputInfoValueTypeList<
  Tuple extends readonly OperationInputInfo[]
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
  OperationInputInfoList extends readonly OperationInputInfo[]
> {
  open: boolean;
  close: () => void;
  title: I18nText | (() => React.ReactNode);
  titleColor?: "default" | "dangerous" | "create" | string;
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
  OperationInputInfoList extends readonly OperationInputInfo[]
>(
  props: OperationDialogProps<TData, OperationInputInfoList>
): React.ReactElement => {
  const inputScheme = props.inputScheme as readonly OperationInputInfo[];

  const { t } = useTranslation();

  type Step =
    | "input"
    | "process"
    | {
        type: "success";
        data: TData;
      }
    | {
        type: "failure";
        data: unknown;
      };
  const [step, setStep] = useState<Step>("input");
  const [values, setValues] = useState<(boolean | string)[]>(
    inputScheme.map((i) => {
      if (i.type === "bool") {
        return i.initValue ?? false;
      } else if (i.type === "text" || i.type === "select") {
        return i.initValue ?? "";
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
    if (step !== "process") {
      props.close();
      if (
        typeof step === "object" &&
        step.type === "success" &&
        props.onSuccessAndClose
      ) {
        props.onSuccessAndClose(step.data);
      }
    } else {
      console.log("Attempt to close modal when processing.");
    }
  };

  const onConfirm = (): void => {
    setStep("process");
    props
      .onProcess(
        (values as unknown) as MapOperationInputInfoValueTypeList<
          OperationInputInfoList
        >
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
  if (step === "input" || step === "process") {
    const process = step === "process";

    let inputPrompt =
      typeof props.inputPrompt === "function"
        ? props.inputPrompt()
        : convertI18nText(props.inputPrompt, t);
    inputPrompt = <h6>{inputPrompt}</h6>;

    const validate = (values: (string | boolean)[]): boolean => {
      const { inputValidator } = props;
      if (inputValidator != null) {
        const result = inputValidator(
          (values as unknown) as MapOperationInputInfoValueTypeList<
            OperationInputInfoList
          >
        );
        setInputError(result);
        return isNoError(result);
      }
      return true;
    };

    const updateValue = (index: number, newValue: string | boolean): void => {
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
            }
          })}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" onClick={close}>
            {t("operationDialog.cancel")}
          </Button>
          <LoadingButton
            variant="primary"
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
          props.titleColor != null
            ? "text-" +
              (props.titleColor === "create"
                ? "success"
                : props.titleColor === "dangerous"
                ? "danger"
                : props.titleColor)
            : undefined
        }
      >
        {title}
      </Modal.Header>
      {body}
    </Modal>
  );
};

export default OperationDialog;
