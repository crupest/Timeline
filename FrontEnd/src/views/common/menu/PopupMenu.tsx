import classNames from "classnames";
import React from "react";
import { createPortal } from "react-dom";
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
  const { styles, attributes } = usePopper(referenceElement, popperElement);

  React.useEffect(() => {
    const handler = (event: MouseEvent): void => {
      let element: HTMLElement | null = event.target as HTMLElement;
      while (element) {
        if (element == referenceElement || element == popperElement) {
          return;
        }
        element = element.parentElement;
      }
      setShow(false);
    };
    document.addEventListener("click", handler);
    return () => {
      document.removeEventListener("click", handler);
    };
  }, [referenceElement, popperElement]);

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
      {show
        ? createPortal(
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
            document.getElementById("portal")!
          )
        : null}
    </>
  );
};

export default PopupMenu;
