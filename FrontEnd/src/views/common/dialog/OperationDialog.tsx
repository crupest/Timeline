import { useState, ReactNode } from "react";
import classNames from "classnames";

import { useC, Text, ThemeColor } from "../common";

import Button from "../button/Button";
import {
  useInputs,
  InputGroup,
  Initializer as InputInitializer,
  InputValueDict,
  InputErrorDict,
} from "../input/InputGroup";
import LoadingButton from "../button/LoadingButton";
import Dialog from "./Dialog";

import "./OperationDialog.css";

export type { InputInitializer, InputValueDict, InputErrorDict };

interface OperationDialogPromptProps {
  message?: Text;
  customMessage?: ReactNode;
  className?: string;
}

function OperationDialogPrompt(props: OperationDialogPromptProps) {
  const { message, customMessage, className } = props;

  const c = useC();

  return (
    <div className={classNames(className, "cru-operation-dialog-prompt")}>
      {message && <p>{c(message)}</p>}
      {customMessage}
    </div>
  );
}

export interface OperationDialogProps<TData> {
  open: boolean;
  close: () => void;

  color?: ThemeColor;
  inputColor?: ThemeColor;
  title: Text;
  inputPrompt?: Text;
  successPrompt?: (data: TData) => ReactNode;
  failurePrompt?: (error: unknown) => ReactNode;

  inputs: InputInitializer;

  onProcess: (inputs: InputValueDict) => Promise<TData>;
  onSuccessAndClose?: (data: TData) => void;
}

function OperationDialog<TData>(props: OperationDialogProps<TData>) {
  const {
    open,
    close,
    color,
    inputColor,
    title,
    inputPrompt,
    successPrompt,
    failurePrompt,
    inputs,
    onProcess,
    onSuccessAndClose,
  } = props;

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

  const { inputGroupProps, hasError, setAllDisabled, confirm } = useInputs({
    init: inputs,
  });

  function onClose() {
    if (step.type !== "process") {
      close();
      if (step.type === "success" && onSuccessAndClose) {
        onSuccessAndClose?.(step.data);
      }
    } else {
      console.log("Attempt to close modal dialog when processing.");
    }
  }

  function onConfirm() {
    const result = confirm();
    if (result.type === "ok") {
      setStep({ type: "process" });
      setAllDisabled(true);
      onProcess(result.values).then(
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
  }

  let body: ReactNode;
  if (step.type === "input" || step.type === "process") {
    const isProcessing = step.type === "process";

    body = (
      <div className="cru-operation-dialog-main-area">
        <div className="cru-dialog-middle-area">
          <OperationDialogPrompt customMessage={c(inputPrompt)} />
          <InputGroup
            containerClassName="cru-operation-dialog-input-group"
            color={inputColor ?? "primary"}
            {...inputGroupProps}
          />
        </div>
        <hr />
        <div className="cru-dialog-bottom-area">
          <Button
            text="operationDialog.cancel"
            color="secondary"
            outline
            onClick={onClose}
            disabled={isProcessing}
          />
          <LoadingButton
            color={color}
            loading={isProcessing}
            disabled={hasError}
            onClick={onConfirm}
          >
            {c("operationDialog.confirm")}
          </LoadingButton>
        </div>
      </div>
    );
  } else {
    const result = step;

    const promptProps: OperationDialogPromptProps =
      result.type === "success"
        ? {
            message: "operationDialog.success",
            customMessage: successPrompt?.(result.data),
          }
        : {
            message: "operationDialog.error",
            customMessage: failurePrompt?.(result.data),
          };
    body = (
      <div className="cru-operation-dialog-main-area">
        <OperationDialogPrompt {...promptProps} />
        <hr />
        <div className="cru-dialog-bottom-area">
          <Button text="operationDialog.ok" color="primary" onClick={onClose} />
        </div>
      </div>
    );
  }

  return (
    <Dialog open={open} onClose={onClose}>
      <div
        className={classNames(
          "cru-operation-dialog-container",
          `cru-${color ?? "primary"}`,
        )}
      >
        <div className="cru-operation-dialog-title">{c(title)}</div>
        <hr />
        {body}
      </div>
    </Dialog>
  );
}

export default OperationDialog;
