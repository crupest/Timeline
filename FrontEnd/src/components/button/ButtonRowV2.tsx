import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import { Text, ClickableColor } from "../common";

import Button from "./Button";
import FlatButton from "./FlatButton";
import IconButton from "./IconButton";
import LoadingButton from "./LoadingButton";

import "./ButtonRow.css";

type ButtonAction = "major" | "minor";

interface ButtonRowV2ButtonBase {
  key: string | number;
  action?: ButtonAction;
  color?: ClickableColor;
  disabled?: boolean;
  onClick?: () => void;
}

interface ButtonRowV2ButtonWithNoType extends ButtonRowV2ButtonBase {
  type?: undefined | null;
  text: Text;
  outline?: boolean;
  props?: ComponentPropsWithoutRef<typeof Button>;
}

interface ButtonRowV2NormalButton extends ButtonRowV2ButtonBase {
  type: "normal";
  text: Text;
  outline?: boolean;
  props?: ComponentPropsWithoutRef<typeof Button>;
}

interface ButtonRowV2FlatButton extends ButtonRowV2ButtonBase {
  type: "flat";
  text: Text;
  props?: ComponentPropsWithoutRef<typeof FlatButton>;
}

interface ButtonRowV2IconButton extends ButtonRowV2ButtonBase {
  type: "icon";
  icon: string;
  props?: ComponentPropsWithoutRef<typeof IconButton>;
}

interface ButtonRowV2LoadingButton extends ButtonRowV2ButtonBase {
  type: "loading";
  text: Text;
  loading?: boolean;
  props?: ComponentPropsWithoutRef<typeof LoadingButton>;
}

type ButtonRowV2Button =
  | ButtonRowV2ButtonWithNoType
  | ButtonRowV2NormalButton
  | ButtonRowV2FlatButton
  | ButtonRowV2IconButton
  | ButtonRowV2LoadingButton;

interface ButtonRowV2Props {
  className?: string;
  containerRef?: Ref<HTMLDivElement>;
  buttons: ButtonRowV2Button[];
  buttonsClassName?: string;
}

export default function ButtonRowV2({
  className,
  containerRef,
  buttons,
  buttonsClassName,
}: ButtonRowV2Props) {
  return (
    <div ref={containerRef} className={classNames("cru-button-row", className)}>
      {buttons.map((button) => {
        const { key, action, color, disabled, onClick } = button;

        const realAction: ButtonAction = action ?? "minor";
        const realColor =
          color ?? (realAction === "major" ? "primary" : "minor");

        const commonProps = { key, color: realColor, disabled, onClick };
        const newClassName = classNames(
          button.props?.className,
          buttonsClassName,
        );

        switch (button.type) {
          case null:
          case undefined:
          case "normal": {
            const { text, outline, props } = button;
            return (
              <Button
                {...commonProps}
                text={text}
                outline={outline ?? realAction !== "major"}
                {...props}
                className={newClassName}
              />
            );
          }
          case "flat": {
            const { text, props } = button;
            return (
              <FlatButton
                {...commonProps}
                text={text}
                {...props}
                className={newClassName}
              />
            );
          }
          case "icon": {
            const { icon, props } = button;
            return (
              <IconButton
                {...commonProps}
                icon={icon}
                {...props}
                className={newClassName}
              />
            );
          }
          case "loading": {
            const { text, loading, props } = button;
            return (
              <LoadingButton
                {...commonProps}
                text={text}
                loading={loading}
                {...props}
                className={newClassName}
              />
            );
          }
          default:
            throw new Error();
        }
      })}
    </div>
  );
}
