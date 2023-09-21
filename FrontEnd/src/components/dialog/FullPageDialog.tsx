import { ReactNode, useRef } from "react";
import classNames from "classnames";
import { CSSTransition } from "react-transition-group";

import { ThemeColor } from "../common";
import { IconButton } from "../button";

import "./FullPageDialog.css";

interface FullPageDialogProps {
  open: boolean;
  onClose: () => void;
  color?: ThemeColor;
  contentContainerClassName?: string;
  children: ReactNode;
}

export default function FullPageDialog({
  open,
  onClose,
  color,
  children,
  contentContainerClassName,
}: FullPageDialogProps) {
  const nodeRef = useRef(null);

  return (
    <CSSTransition
      nodeRef={nodeRef}
      mountOnEnter
      unmountOnExit
      in={open}
      timeout={300}
      classNames="cru-dialog-full-page"
    >
      <div
        ref={nodeRef}
        className={`cru-dialog-full-page cru-theme-${color ?? "primary"}`}
      >
        <div className="cru-dialog-full-page-top-bar">
          <IconButton
            icon="arrow-left"
            color="light"
            className="cru-dialog-full-page-back-button"
            onClick={onClose}
          />
        </div>
        <div
          className={classNames(
            "cru-dialog-full-page-content-container",
            contentContainerClassName,
          )}
        >
          {children}
        </div>
      </div>
    </CSSTransition>
  );
}
