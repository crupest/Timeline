import { CSSProperties } from "react";
import classNames from "classnames";

import { useC, Text, ThemeColor } from "../common";

import "./Menu.css";
import Icon from "../Icon";

export type MenuItem =
  | {
      type: "divider";
    }
  | {
      type: "button";
      text: Text;
      icon?: string;
      color?: ThemeColor;
      onClick: () => void;
    };

export type MenuItems = MenuItem[];

export type MenuProps = {
  color?: ThemeColor;
  items: MenuItems;
  onItemClicked?: () => void;
  className?: string;
  style?: CSSProperties;
};

export default function Menu({
  color,
  items,
  onItemClicked,
  className,
  style,
}: MenuProps) {
  const c = useC();

  return (
    <div
      className={classNames(`cru-menu cru-button-${color ?? "primary"}`, className)}
      style={style}
    >
      {items.map((item, index) => {
        if (item.type === "divider") {
          return <hr key={index} className="cru-menu-divider" />;
        } else {
          const { text, color, icon, onClick } = item;
          return (
            <div
              key={index}
              className={`cru-menu-item cru-button-${color ?? "primary"}`}
              onClick={() => {
                onClick();
                onItemClicked?.();
              }}
            >
              {icon != null && <Icon color={color} icon={icon} />}
              {c(text)}
            </div>
          );
        }
      })}
    </div>
  );
}
