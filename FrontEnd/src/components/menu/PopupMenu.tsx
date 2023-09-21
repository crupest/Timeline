import { useState, CSSProperties, ReactNode } from "react";
import classNames from "classnames";
import { usePopper } from "react-popper";

import { ThemeColor } from "../common";
import { useClickOutside } from "../hooks";
import Menu, { MenuItems } from "./Menu";

import "./PopupMenu.css";

export interface PopupMenuProps {
  color?: ThemeColor;
  items: MenuItems;
  children?: ReactNode;
  containerClassName?: string;
  containerStyle?: CSSProperties;
}

export default function PopupMenu({
  color,
  items,
  children,
  containerClassName,
  containerStyle,
}: PopupMenuProps) {
  const [show, setShow] = useState<boolean>(false);

  const [referenceElement, setReferenceElement] =
    useState<HTMLDivElement | null>(null);
  const [popperElement, setPopperElement] = useState<HTMLDivElement | null>(
    null,
  );
  const { styles, attributes } = usePopper(referenceElement, popperElement);

  useClickOutside(popperElement, () => setShow(false), true);

  return (
    <div
      ref={setReferenceElement}
      className={classNames(
        "cru-popup-menu-trigger-container",
        containerClassName,
      )}
      style={containerStyle}
      onClick={() => setShow(true)}
    >
      {children}
      {show && (
        <div
          ref={setPopperElement}
          className={`cru-popup-menu-menu-container cru-clickable-${
            color ?? "primary"
          }`}
          style={styles.popper}
          {...attributes.popper}
        >
          <Menu
            items={items}
            onItemClick={(e) => {
              setShow(false);
              e.stopPropagation();
            }}
          />
        </div>
      )}
    </div>
  );
}
