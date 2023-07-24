import { useState, ReactNode } from "react";
import classNames from "classnames";

import { useC, Text, ThemeColor } from "../common";

import Button from "../button/Button";
import {
  default as InputGroup,
  InputErrors,
  InputList,
  Validator,
  Values,
  useDirties,
} from "../input/InputGroup";
import LoadingButton from "../button/LoadingButton";
import Dialog from "./Dialog";

import "./OperationDialog.css";

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

export interface OperationDialogProps<TData, Inputs extends InputList> {
  open: boolean;
  onClose: () => void;

  color?: ThemeColor;
  title: Text;
  inputPrompt?: Text;
  processPrompt?: Text;
  successPrompt?: (data: TData) => ReactNode;
  failurePrompt?: (error: unknown) => ReactNode;

  inputs: Inputs;
  validator?: Validator<Inputs>;

  onProcess: (inputs: Values<Inputs>) => Promise<TData>;
  onSuccessAndClose?: (data: TData) => void;
}

function OperationDialog<TData, Inputs extends InputList>(
  props: OperationDialogProps<TData, Inputs>,
) {
  const {
    open,
    onClose,
    color,
    title,
    inputPrompt,
    processPrompt,
    successPrompt,
    failurePrompt,
    inputs,
    validator,
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
  const [values, setValues] = useState<Values<Inputs>>();
  const [errors, setErrors] = useState<InputErrors>();
  const [dirties, setDirties, dirtyAll] = useDirties();

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
    const isProcessing = step.type === "process";
    const hasError = errors.length > 0;

    body = (
      <div className="cru-operation-dialog-main-area">
        <div>
          <OperationDialogPrompt customMessage={c(props.inputPrompt)} />
          <InputGroup
            className="cru-operation-dialog-input-group"
            color={color}
            inputs={inputs}
            validator={validator}
            values={values}
            errors={errors}
            disabled={isProcessing}
            onChange={(values, errors) => {
              setValues(values);
              setErrors(errors);
            }}
            dirties={dirties}
            onDirty={setDirties}
          />
        </div>
        <hr />
        <div className="cru-dialog-bottom-area">
          <Button
            text="operationDialog.cancel"
            color="secondary"
            outline
            onClick={close}
            disabled={isProcessing}
          />
          <LoadingButton
            color={color}
            loading={isProcessing}
            disabled={hasError}
            onClick={() => {
              dirtyAll();
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

    const promptProps: OperationDialogPromptProps =
      result.type === "success"
        ? {
            message: "operationDialog.success",
            customMessage: props.successPrompt?.(result.data),
          }
        : {
            message: "operationDialog.error",
            customMessage: props.failurePrompt?.(result.data),
          };
    body = (
      <div className="cru-operation-dialog-main-area">
        <OperationDialogPrompt {...promptProps} />
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
        className={classNames(
          "cru-operation-dialog-container",
          `cru-${props.themeColor ?? "primary"}`,
        )}
      >
        <div className="cru-operation-dialog-title">{c(props.title)}</div>
        <hr />
        {body}
      </div>
    </Dialog>
  );
}

export default OperationDialog;
