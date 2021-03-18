import React from "react";
import clsx from "clsx";
import { OverlayTrigger, OverlayTriggerProps, Popover } from "react-bootstrap";
import { useTranslation } from "react-i18next";

import { BootstrapThemeColor, convertI18nText, I18nText } from "@/common";

export type MenuItem =
  | {
      type: "divider";
    }
  | {
      type: "button";
      text: I18nText;
      iconClassName?: string;
      color?: BootstrapThemeColor;
      onClick: () => void;
    };

export type MenuItems = MenuItem[];

export interface MenuProps {
  items: MenuItems;
  className?: string;
  onItemClicked?: () => void;
}

const Menu: React.FC<MenuProps> = ({ items, className, onItemClicked }) => {
  const { t } = useTranslation();

  return (
    <div className={clsx("cru-menu", className)}>
      {items.map((item, index) => {
        if (item.type === "divider") {
          return <div key={index} className="cru-menu-divider" />;
        } else {
          return (
            <div
              key={index}
              className={clsx(
                "cru-menu-item",
                `color-${item.color ?? "primary"}`
              )}
              onClick={() => {
                item.onClick();
                onItemClicked?.();
              }}
            >
              {item.iconClassName != null ? (
                <i className={clsx(item.iconClassName, "cru-menu-item-icon")} />
              ) : null}
              {convertI18nText(item.text, t)}
            </div>
          );
        }
      })}
    </div>
  );
};

export default Menu;

export interface PopupMenuProps {
  items: MenuItems;
  children: OverlayTriggerProps["children"];
}

export const PopupMenu: React.FC<PopupMenuProps> = ({ items, children }) => {
  const [show, setShow] = React.useState<boolean>(false);
  const toggle = (): void => setShow(!show);

  return (
    <OverlayTrigger
      trigger="click"
      rootClose
      overlay={
        <Popover id="menu-popover">
          <Menu items={items} onItemClicked={() => setShow(false)} />
        </Popover>
      }
      show={show}
      onToggle={toggle}
    >
      {children}
    </OverlayTrigger>
  );
};
