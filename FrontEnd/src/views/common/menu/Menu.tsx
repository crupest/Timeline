import React from "react";
import classnames from "classnames";
import { useTranslation } from "react-i18next";

import { convertI18nText, I18nText } from "@/common";
import { PaletteColorType } from "@/palette";

import "./Menu.css";

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

export type MenuProps = {
  items: MenuItems;
  onItemClicked?: () => void;
  className?: string;
  style?: React.CSSProperties;
};

export default function _Menu({
  items,
  onItemClicked,
  className,
  style,
}: MenuProps): React.ReactElement | null {
  const { t } = useTranslation();

  return (
    <div className={classnames("cru-menu", className)} style={style}>
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
}
