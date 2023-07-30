import { ComponentProps, Ref, ReactNode } from "react";
import classNames from "classnames";

import { ThemeColor, Text, useC } from "../common";
import { ButtonRow } from "../button";

import "./DialogContainer.css";

interface DialogContainerProps {
  className?: string;
  title: Text;
  titleColor?: ThemeColor;
  titleClassName?: string;
  titleRef?: Ref<HTMLDivElement>;
  bodyContainerClassName?: string;
  bodyContainerRef?: Ref<HTMLDivElement>;
  buttons: ComponentProps<typeof ButtonRow>["buttons"];
  buttonsClassName?: string;
  buttonsContainerRef?: ComponentProps<typeof ButtonRow>["containerRef"];
  children: ReactNode;
}

export default function DialogContainer({
  className,
  title,
  titleColor,
  titleClassName,
  titleRef,
  bodyContainerClassName,
  bodyContainerRef,
  buttons,
  buttonsClassName,
  buttonsContainerRef,
  children,
}: DialogContainerProps) {
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
      <ButtonRow
        containerRef={buttonsContainerRef}
        className={classNames("cru-dialog-container-button-row", buttonsClassName)}
        buttons={buttons}
        buttonsClassName="cru-dialog-container-button"
      />
    </div>
  );
}
