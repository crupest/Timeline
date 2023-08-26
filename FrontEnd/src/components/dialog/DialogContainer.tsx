import { ComponentProps, Ref, ReactNode } from "react";
import classNames from "classnames";

import { ThemeColor, Text, useC } from "../common";
import { ButtonRow, ButtonRowV2 } from "../button";

import "./DialogContainer.css";

interface DialogContainerBaseProps {
  className?: string;
  title: Text;
  titleColor?: ThemeColor;
  titleClassName?: string;
  titleRef?: Ref<HTMLDivElement>;
  bodyContainerClassName?: string;
  bodyContainerRef?: Ref<HTMLDivElement>;
  buttonsClassName?: string;
  buttonsContainerRef?: ComponentProps<typeof ButtonRow>["containerRef"];
  children: ReactNode;
}

interface DialogContainerWithButtonsProps extends DialogContainerBaseProps {
  buttons: ComponentProps<typeof ButtonRow>["buttons"];
}

interface DialogContainerWithButtonsV2Props extends DialogContainerBaseProps {
  buttonsV2: ComponentProps<typeof ButtonRowV2>["buttons"];
}

type DialogContainerProps =
  | DialogContainerWithButtonsProps
  | DialogContainerWithButtonsV2Props;

export default function DialogContainer(props: DialogContainerProps) {
  const {
    className,
    title,
    titleColor,
    titleClassName,
    titleRef,
    bodyContainerClassName,
    bodyContainerRef,
    buttonsClassName,
    buttonsContainerRef,
    children,
  } = props;

  const c = useC();

  return (
    <div className={classNames(className)}>
      <div
        ref={titleRef}
        className={classNames(
          `cru-dialog-container-title cru-${titleColor ?? "primary"}`,
          titleClassName,
        )}
      >
        {c(title)}
      </div>
      <hr className="cru-dialog-container-hr" />
      <div
        ref={bodyContainerRef}
        className={classNames(
          "cru-dialog-container-body",
          bodyContainerClassName,
        )}
      >
        {children}
      </div>
      <hr className="cru-dialog-container-hr" />
      {"buttons" in props ? (
        <ButtonRow
          containerRef={buttonsContainerRef}
          className={classNames(
            "cru-dialog-container-button-row",
            buttonsClassName,
          )}
          buttons={props.buttons}
          buttonsClassName="cru-dialog-container-button"
        />
      ) : (
        <ButtonRowV2
          containerRef={buttonsContainerRef}
          className={classNames(
            "cru-dialog-container-button-row",
            buttonsClassName,
          )}
          buttons={props.buttonsV2}
          buttonsClassName="cru-dialog-container-button"
        />
      )}
    </div>
  );
}
