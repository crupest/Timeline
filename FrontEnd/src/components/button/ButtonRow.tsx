import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

import Button from "./Button";
import FlatButton from "./FlatButton";
import IconButton from "./IconButton";
import LoadingButton from "./LoadingButton";

import "./ButtonRow.css";

type ButtonRowButton = (
  | {
      type: "normal";
      props: ComponentPropsWithoutRef<typeof Button>;
    }
  | {
      type: "flat";
      props: ComponentPropsWithoutRef<typeof FlatButton>;
    }
  | {
      type: "icon";
      props: ComponentPropsWithoutRef<typeof IconButton>;
    }
  | { type: "loading"; props: ComponentPropsWithoutRef<typeof LoadingButton> }
) & { key: string | number };

interface ButtonRowProps {
  className?: string;
  containerRef?: Ref<HTMLDivElement>;
  buttons: ButtonRowButton[];
  buttonsClassName?: string;
}

export default function ButtonRow({
  className,
  containerRef,
  buttons,
  buttonsClassName,
}: ButtonRowProps) {
  return (
    <div ref={containerRef} className={classNames("cru-button-row", className)}>
      {buttons.map((button) => {
        const { type, key, props } = button;
        const newClassName = classNames(props.className, buttonsClassName);
        switch (type) {
          case "normal":
            return <Button key={key} {...props} className={newClassName} />;
          case "flat":
            return <FlatButton key={key} {...props} className={newClassName} />;
          case "icon":
            return <IconButton key={key} {...props} className={newClassName} />;
          case "loading":
            return (
              <LoadingButton key={key} {...props} className={newClassName} />
            );
          default:
            throw new Error();
        }
      })}
    </div>
  );
}
