import { useState, ReactNode, ComponentProps } from "react";
import classNames from "classnames";

import { useC, Text, ThemeColor } from "../common";
import {
  useInputs,
  InputGroup,
  Initializer as InputInitializer,
  InputConfirmValueDict,
} from "../input";
import { ButtonRow } from "../button";
import Dialog from "./Dialog";
import DialogContainer from "./DialogContainer";
import { useDialogController } from "./DialogProvider";

import "./OperationDialog.css";

interface OperationDialogPromptProps {
  message?: Text;
  customMessage?: Text;
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
  color?: ThemeColor;
  inputColor?: ThemeColor;
  title: Text;
  inputPrompt?: Text;
  inputPromptNode?: ReactNode;
  successPrompt?: (data: TData) => Text;
  successPromptNode?: (data: TData) => ReactNode;
  failurePrompt?: (error: unknown) => Text;
  failurePromptNode?: (error: unknown) => ReactNode;

  inputs: InputInitializer;

  onProcess: (inputs: InputConfirmValueDict) => Promise<TData>;
  onSuccessAndClose?: (data: TData) => void;
}

function OperationDialog<TData>(props: OperationDialogProps<TData>) {
  const {
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

  const dialogController = useDialogController();

  const [step, setStep] = useState<Step>({ type: "input" });

  const { inputGroupProps, hasErrorAndDirty, setAllDisabled, confirm } =
    useInputs({
      init: inputs,
    });

  function close() {
    if (step.type !== "process") {
      dialogController.closeDialog();
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
      dialogController.setCanSwitchDialog(false);
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
        .finally(() => {
          dialogController.setCanSwitchDialog(true);
        });
    }
  }

  let body: ReactNode;
  let buttons: ComponentProps<typeof ButtonRow>["buttons"];

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
        props: {
          text: "operationDialog.ok",
          color: "primary",
          onClick: close,
        },
      },
    ];
  }

  return (
    <Dialog color={color}>
      <DialogContainer title={title} titleColor={color} buttons={buttons}>
        {body}
      </DialogContainer>
    </Dialog>
  );
}

export default OperationDialog;
