import React from "react";
import classnames from "classnames";
import { useTranslation } from "react-i18next";

import { convertI18nText, I18nText } from "@/common";
import { PaletteColorType } from "@/palette";

export type MenuItem =
  | {
      type: "divider";
    }
  | {
      type: "button";
      text: I18nText;
      iconClassName?: string;
      color?: PaletteColorType;
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
    <div className={classnames("cru-menu", className)}>
      {items.map((item, index) => {
        if (item.type === "divider") {
          return <div key={index} className="cru-menu-divider" />;
        } else {
          return (
            <div
              key={index}
              className={classnames(
                "cru-menu-item",
                `color-${item.color ?? "primary"}`
              )}
              onClick={() => {
                item.onClick();
                onItemClicked?.();
              }}
            >
              {item.iconClassName != null ? (
                <i
                  className={classnames(
                    item.iconClassName,
                    "cru-menu-item-icon"
                  )}
                />
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
  children: React.ReactElement;
}

export const PopupMenu: React.FC<PopupMenuProps> = ({ items, children }) => {
  const [show, setShow] = React.useState<boolean>(false);
  const toggle = (): void => setShow(!show);

  // TODO:

  return <Menu items={items} onItemClicked={() => setShow(false)} />;
};
