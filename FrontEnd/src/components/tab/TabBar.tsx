import { ReactNode } from "react";
import { Link } from "react-router-dom";
import classNames from "classnames";

import { Text, ThemeColor, useC } from "../common";

import "./TabBar.css";

export interface Tab {
  name: string;
  text: Text;
  link?: string;
  onClick?: () => void;
}

export interface TabsProps {
  activeTabName?: string;
  tabs: Tab[];
  color?: ThemeColor;
  actions?: ReactNode;
  dense?: boolean;
  className?: string;
}

export default function TabBar(props: TabsProps) {
  const { tabs, color, activeTabName, className, dense, actions } = props;

  const c = useC();

  return (
    <div
      className={classNames(
        "cru-tab-bar",
        dense && "dense",
        `cru-clickable-${color ?? "primary"}`,
        className,
      )}
    >
      <div className="cru-tab-bar-tab-area">
        {tabs.map((tab) => {
          const { name, text, link, onClick } = tab;

          const active = activeTabName === name;
          const className = classNames("cru-tab-bar-item", active && "active");

          if (link != null) {
            return (
              <Link
                key={name}
                to={link}
                onClick={onClick}
                className={className}
              >
                {c(text)}
              </Link>
            );
          } else {
            return (
              <span key={name} onClick={onClick} className={className}>
                {c(text)}
              </span>
            );
          }
        })}
      </div>
      <div className="cru-tab-bar-action-area">{actions}</div>
    </div>
  );
}
