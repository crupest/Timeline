import { useState, ReactNode, ComponentProps } from "react";
import classNames from "classnames";

import { useC, I18nText, ThemeColor } from "../common";
import {
  useInputs,
  InputGroup,
  Initializer as InputInitializer,
  InputConfirmValueDict,
} from "../input";
import { ButtonRowV2 } from "../button";
import Dialog from "./Dialog";
import DialogContainer from "./DialogContainer";

import "./OperationDialog.css";

interface OperationDialogPromptProps {
  message?: I18nText;
  customMessage?: I18nText;
  customMessageNode?: ReactNode;
  className?: string;
}

function OperationDialogPrompt(props: OperationDialogPromptProps) {
  const { message, customMessage, customMessageNode, className } = props;

  const c = useC();

  return (
    <div className={classNames(className, "cru-operation-dialog-prompt")}>
      {message && <p>{c(message)}</p>}
      {customMessageNode ?? (customMessage != null ? c(customMessage) : null)}
    </div>
  );
}

export interface OperationDialogProps<TData> {
  open: boolean;
  onClose: () => void;
  color?: ThemeColor;
  inputColor?: ThemeColor;
  title: I18nText;
  inputPrompt?: I18nText;
  inputPromptNode?: ReactNode;
  successPrompt?: (data: TData) => I18nText;
  successPromptNode?: (data: TData) => ReactNode;
  failurePrompt?: (error: unknown) => I18nText;
  failurePromptNode?: (error: unknown) => ReactNode;

  inputs: InputInitializer;

  onProcess: (inputs: InputConfirmValueDict) => Promise<TData>;
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
    inputPromptNode,
    successPrompt,
    successPromptNode,
    failurePrompt,
    failurePromptNode,
    inputs,
    onProcess,
    onSuccessAndClose,
  } = props;

  if (process.env.NODE_ENV === "development") {
    if (inputPrompt && inputPromptNode) {
      console.log("InputPrompt and inputPromptNode are both set.");
    }
    if (successPrompt && successPromptNode) {
      console.log("SuccessPrompt and successPromptNode are both set.");
    }
    if (failurePrompt && failurePromptNode) {
      console.log("FailurePrompt and failurePromptNode are both set.");
    }
  }

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
      onProcess(result.values)
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
        )
    }
  }

  let body: ReactNode;
  let buttons: ComponentProps<typeof ButtonRowV2>["buttons"];

  if (step.type === "input" || step.type === "process") {
    const isProcessing = step.type === "process";

    body = (
      <div>
        <OperationDialogPrompt
          customMessage={inputPrompt}
          customMessageNode={inputPromptNode}
        />
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
        text: "operationDialog.cancel",
        onClick: close,
        disabled: isProcessing,
      },
      {
        key: "confirm",
        type: "loading",
        action: "major",
        text: "operationDialog.confirm",
        color,
        loading: isProcessing,
        disabled: hasErrorAndDirty,
        onClick: onConfirm,
      },
    ];
  } else {
    const result = step;

    const promptProps: OperationDialogPromptProps =
      result.type === "success"
        ? {
            message: "operationDialog.success",
            customMessage: successPrompt?.(result.data),
            customMessageNode: successPromptNode?.(result.data),
          }
        : {
            message: "operationDialog.error",
            customMessage: failurePrompt?.(result.data),
            customMessageNode: failurePromptNode?.(result.data),
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
        action: "major",
        color: "create",
        text: "operationDialog.ok",
        onClick: close,
      },
    ];
  }

  return (
    <Dialog open={open} onClose={close} color={color}>
      <DialogContainer title={title} titleColor={color} buttonsV2={buttons}>
        {body}
      </DialogContainer>
    </Dialog>
  );
}

export default OperationDialog;
