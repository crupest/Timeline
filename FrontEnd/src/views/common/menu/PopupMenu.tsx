import { useState, CSSProperties, ReactNode } from "react";
import classNames from "classnames";
import { createPortal } from "react-dom";
import { usePopper } from "react-popper";

import { useClickOutside } from "@/utilities/hooks";

import Menu, { MenuItems } from "./Menu";

import "./PopupMenu.css";

export interface PopupMenuProps {
  items: MenuItems;
  children?: ReactNode;
  containerClassName?: string;
  containerStyle?: CSSProperties;
}

export default function PopupMenu({
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
      {show &&
        createPortal(
          <div
            ref={setPopperElement}
            className="cru-popup-menu-menu-container"
            style={styles.popper}
            {...attributes.popper}
          >
            <Menu
              items={items}
              onItemClicked={() => {
                setShow(false);
              }}
            />
          </div>,
          // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
          document.getElementById("portal")!,
        )}
    </div>
  );
}
