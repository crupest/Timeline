import classNames from "classnames";
import React from "react";
import { usePopper } from "react-popper";

import Menu, { MenuItems } from "./Menu";

import "./PopupMenu.css";

export interface PopupMenuProps {
  items: MenuItems;
  children?: React.ReactNode;
  containerClassName?: string;
  containerStyle?: React.CSSProperties;
}

const PopupMenu: React.FC<PopupMenuProps> = ({
  items,
  children,
  containerClassName,
  containerStyle,
}) => {
  const [show, setShow] = React.useState<boolean>(false);

  const [referenceElement, setReferenceElement] =
    React.useState<HTMLDivElement | null>(null);
  const [popperElement, setPopperElement] =
    React.useState<HTMLDivElement | null>(null);
  const [arrowElement, setArrowElement] = React.useState<HTMLDivElement | null>(
    null
  );
  const { styles, attributes } = usePopper(referenceElement, popperElement, {
    modifiers: [{ name: "arrow", options: { element: arrowElement } }],
  });

  return (
    <>
      <div
        ref={setReferenceElement}
        className={classNames(
          "cru-popup-menu-trigger-container",
          containerClassName
        )}
        style={containerStyle}
        onClick={() => setShow(true)}
      >
        {children}
      </div>
      {show ? (
        <div
          ref={setPopperElement}
          className="cru-popup-menu-menu-container"
          style={styles.popper}
          {...attributes.popper}
        >
          <Menu items={items} />
          <div ref={setArrowElement} style={styles.arrow} />
        </div>
      ) : null}
    </>
  );
};

export default PopupMenu;
