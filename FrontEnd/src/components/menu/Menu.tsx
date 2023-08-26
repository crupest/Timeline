import { CSSProperties } from "react";
import classNames from "classnames";

import { useC, Text, ThemeColor } from "../common";
import Icon from "../Icon";

import "./Menu.css";

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
  items: MenuItems;
  onItemClicked?: () => void;
  className?: string;
  style?: CSSProperties;
};

export default function Menu({
  items,
  onItemClicked,
  className,
  style,
}: MenuProps) {
  const c = useC();

  return (
    <div className={classNames("cru-menu", className)} style={style}>
      {items.map((item, index) => {
        if (item.type === "divider") {
          return <hr key={index} className="cru-menu-divider" />;
        } else {
          const { text, color, icon, onClick } = item;
          return (
            <button
              key={index}
              className={`cru-menu-item cru-clickable-${color ?? "primary"}`}
              onClick={() => {
                onClick();
                onItemClicked?.();
              }}
            >
              {icon != null && <Icon color={color} icon={icon} />}
              {c(text)}
            </button>
          );
        }
      })}
    </div>
  );
}
