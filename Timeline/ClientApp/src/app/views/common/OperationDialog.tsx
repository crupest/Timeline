import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { Spinner, Container, Form, Button, Modal } from "react-bootstrap";

import { UiLogicError } from "@/common";

const DefaultProcessPrompt: React.FC = (_) => {
  return (
    <Container className="justify-content-center align-items-center">
      <Spinner animation="border" variant="success" />
    </Container>
  );
};

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

export type OperationInputOptionalError = undefined | null | string;

export interface OperationInputErrorInfo {
  [index: number]: OperationInputOptionalError;
}

export type OperationInputValidator<TValue> = (
  value: TValue,
  values: (string | boolean)[]
) => OperationInputOptionalError | OperationInputErrorInfo;

export interface OperationTextInputInfo {
  type: "text";
  password?: boolean;
  label?: string;
  initValue?: string;
  textFieldProps?: Omit<
    React.InputHTMLAttributes<HTMLInputElement>,
    "type" | "value" | "onChange" | "aria-relevant"
  >;
  helperText?: string;
  validator?: OperationInputValidator<string>;
}

export interface OperationBoolInputInfo {
  type: "bool";
  label: string;
  initValue?: boolean;
}

export interface OperationSelectInputInfoOption {
  value: string;
  label: string;
  icon?: React.ReactElement;
}

export interface OperationSelectInputInfo {
  type: "select";
  label: string;
  options: OperationSelectInputInfoOption[];
  initValue?: string;
}

export type OperationInputInfo =
  | OperationTextInputInfo
  | OperationBoolInputInfo
  | OperationSelectInputInfo;

interface OperationResult {
  type: "success" | "failure";
  data: unknown;
}

interface OperationDialogProps {
  open: boolean;
  close: () => void;
  title: React.ReactNode;
  titleColor?: "default" | "dangerous" | "create" | string;
  onProcess: (inputs: (string | boolean)[]) => Promise<unknown>;
  inputScheme?: OperationInputInfo[];
  inputPrompt?: string | (() => React.ReactNode);
  processPrompt?: () => React.ReactNode;
  successPrompt?: (data: unknown) => React.ReactNode;
  failurePrompt?: (error: unknown) => React.ReactNode;
  onSuccessAndClose?: () => void;
}

const OperationDialog: React.FC<OperationDialogProps> = (props) => {
  const inputScheme = props.inputScheme ?? [];

  const { t } = useTranslation();

  type Step = "input" | "process" | OperationResult;
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
  const [inputError, setInputError] = useState<OperationInputErrorInfo>({});

  const close = (): void => {
    if (step !== "process") {
      props.close();
      if (
        typeof step === "object" &&
        step.type === "success" &&
        props.onSuccessAndClose
      ) {
        props.onSuccessAndClose();
      }
    } else {
      console.log("Attempt to close modal when processing.");
    }
  };

  const onConfirm = (): void => {
    setStep("process");
    props.onProcess(values).then(
      (d: unknown) => {
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
  if (step === "input") {
    let inputPrompt =
      typeof props.inputPrompt === "function"
        ? props.inputPrompt()
        : props.inputPrompt;
    inputPrompt = <h6>{inputPrompt}</h6>;

    const updateValue = (
      index: number,
      newValue: string | boolean
    ): (string | boolean)[] => {
      const oldValues = values;
      const newValues = oldValues.slice();
      newValues[index] = newValue;
      setValues(newValues);
      return newValues;
    };

    const testErrorInfo = (errorInfo: OperationInputErrorInfo): boolean => {
      for (let i = 0; i < inputScheme.length; i++) {
        if (inputScheme[i].type === "text" && errorInfo[i] != null) {
          return true;
        }
      }
      return false;
    };

    const calculateError = (
      oldError: OperationInputErrorInfo,
      index: number,
      newError: OperationInputOptionalError | OperationInputErrorInfo
    ): OperationInputErrorInfo => {
      if (newError === undefined) {
        return oldError;
      } else if (newError === null || typeof newError === "string") {
        return { ...oldError, [index]: newError };
      } else {
        const newInputError: OperationInputErrorInfo = { ...oldError };
        for (const [index, error] of Object.entries(newError)) {
          if (error !== undefined) {
            newInputError[+index] = error as OperationInputOptionalError;
          }
        }
        return newInputError;
      }
    };

    const validateAll = (): boolean => {
      let newInputError = inputError;
      for (let i = 0; i < inputScheme.length; i++) {
        const item = inputScheme[i];
        if (item.type === "text") {
          newInputError = calculateError(
            newInputError,
            i,
            item.validator?.(values[i] as string, values)
          );
        }
      }
      const result = !testErrorInfo(newInputError);
      setInputError(newInputError);
      return result;
    };

    body = (
      <>
        <Modal.Body>
          {inputPrompt}
          {inputScheme.map((item, index) => {
            const value = values[index];
            const error: string | undefined = ((e) =>
              typeof e === "string" ? t(e) : undefined)(inputError?.[index]);

            if (item.type === "text") {
              return (
                <Form.Group key={index}>
                  {item.label && <Form.Label>{t(item.label)}</Form.Label>}
                  <Form.Control
                    type={item.password === true ? "password" : "text"}
                    value={value as string}
                    onChange={(e) => {
                      const v = e.target.value;
                      const newValues = updateValue(index, v);
                      setInputError(
                        calculateError(
                          inputError,
                          index,
                          item.validator?.(v, newValues)
                        )
                      );
                    }}
                    isInvalid={error != null}
                  />
                  {error != null && (
                    <Form.Control.Feedback>{error}</Form.Control.Feedback>
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
                    label={t(item.label)}
                  />
                </Form.Group>
              );
            } else if (item.type === "select") {
              return (
                <Form.Group key={index}>
                  <Form.Label>{t(item.label)}</Form.Label>
                  <Form.Control
                    as="select"
                    value={value as string}
                    onChange={(event) => {
                      updateValue(index, event.target.value);
                    }}
                  >
                    {item.options.map((option, i) => {
                      return (
                        <option value={option.value} key={i}>
                          {option.icon}
                          {t(option.label)}
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
          <Button variant="secondary" onClick={close}>
            {t("operationDialog.cancel")}
          </Button>
          <Button
            variant="primary"
            disabled={testErrorInfo(inputError)}
            onClick={() => {
              if (validateAll()) {
                onConfirm();
              }
            }}
          >
            {t("operationDialog.confirm")}
          </Button>
        </Modal.Footer>
      </>
    );
  } else if (step === "process") {
    body = (
      <Modal.Body>
        {props.processPrompt?.() ?? <DefaultProcessPrompt />}
      </Modal.Body>
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

  const title = typeof props.title === "string" ? t(props.title) : props.title;

  return (
    <Modal isOpen={props.open} toggle={close}>
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
