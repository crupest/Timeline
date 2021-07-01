import React from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import classnames from "classnames";

import { convertI18nText, I18nText } from "@/common";

import "./Tabs.css";

export interface Tab {
  name: string;
  text: I18nText;
  link?: string;
  onClick?: () => void;
}

export interface TabsProps {
  activeTabName?: string;
  actions?: React.ReactNode;
  dense?: boolean;
  tabs: Tab[];
  className?: string;
  style?: React.CSSProperties;
}

export default function Tabs(props: TabsProps): React.ReactElement | null {
  const { tabs, activeTabName, className, style, dense, actions } = props;

  const { t } = useTranslation();

  return (
    <div
      className={classnames("cru-nav", dense && "dense", className)}
      style={style}
    >
      {tabs.map((tab) => {
        const active = activeTabName === tab.name;
        const className = classnames("cru-nav-item", active && "active");

        if (tab.link != null) {
          return (
            <Link
              key={tab.name}
              to={tab.link}
              onClick={tab.onClick}
              className={className}
            >
              {convertI18nText(tab.text, t)}
            </Link>
          );
        } else {
          return (
            <span key={tab.name} onClick={tab.onClick} className={className}>
              {convertI18nText(tab.text, t)}
            </span>
          );
        }
      })}
      <div className="cru-nav-action-area">{actions}</div>
    </div>
  );
}
