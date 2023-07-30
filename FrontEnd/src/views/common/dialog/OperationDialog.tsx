import { useState, ReactNode, ComponentProps } from "react";
import classNames from "classnames";

import { useC, Text, ThemeColor } from "../common";

import {
  useInputs,
  InputGroup,
  Initializer as InputInitializer,
  InputValueDict,
  InputErrorDict,
} from "../input/InputGroup";
import Dialog from "./Dialog";
import DialogContainer from "./DialogContainer";

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
  onClose: () => void;

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
    onClose,
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

  const { inputGroupProps, hasErrorAndDirty, setAllDisabled, confirm } =
    useInputs({
      init: inputs,
    });

  function close() {
    if (step.type !== "process") {
      onClose();
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
  let buttons: ComponentProps<typeof DialogContainer>["buttons"];

  if (step.type === "input" || step.type === "process") {
    const isProcessing = step.type === "process";

    body = (
      <div>
        <OperationDialogPrompt customMessage={c(inputPrompt)} />
        <InputGroup
          containerClassName="cru-operation-dialog-input-group"
          color={inputColor ?? "primary"}
          {...inputGroupProps}
        />
      </div>
    );
    buttons = [
      {
        key: "cancel",
        type: "normal",
        props: {
          text: "operationDialog.cancel",
          color: "secondary",
          outline: true,
          onClick: close,
          disabled: isProcessing,
        },
      },
      {
        key: "confirm",
        type: "loading",
        props: {
          text: "operationDialog.confirm",
          color,
          loading: isProcessing,
          disabled: hasErrorAndDirty,
          onClick: onConfirm,
        },
      },
    ];
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
      <div>
        <OperationDialogPrompt {...promptProps} />
      </div>
    );

    buttons = [
      {
        key: "ok",
        type: "normal",
        props: {
          text: "operationDialog.ok",
          color: "primary",
          onClick: close,
        },
      },
    ];
  }

  return (
    <Dialog open={open} onClose={close}>
      <DialogContainer title={title} titleColor={color} buttons={buttons}>
        {body}
      </DialogContainer>
    </Dialog>
  );
}

export default OperationDialog;
