import React from "react";

import { I18nText, UiLogicError } from "@/common";

import Tabs from "./Tabs";

export interface TabPage {
  name: string;
  text: I18nText;
  page: React.ReactNode;
}

export interface TabPagesProps {
  pages: TabPage[];
  actions?: React.ReactNode;
  dense?: boolean;
  className?: string;
  style?: React.CSSProperties;
  navClassName?: string;
  navStyle?: React.CSSProperties;
  pageContainerClassName?: string;
  pageContainerStyle?: React.CSSProperties;
}

const TabPages: React.FC<TabPagesProps> = ({
  pages,
  actions,
  dense,
  className,
  style,
  navClassName,
  navStyle,
  pageContainerClassName,
  pageContainerStyle,
}) => {
  if (pages.length === 0) {
    throw new UiLogicError("Page list can't be empty.");
  }

  const [tab, setTab] = React.useState<string>(pages[0].name);

  const currentPage = pages.find((p) => p.name === tab);

  if (currentPage == null) {
    throw new UiLogicError("Current tab value is bad.");
  }

  return (
    <div className={className} style={style}>
      <Tabs
        tabs={pages.map((page) => ({
          name: page.name,
          text: page.text,
          onClick: () => {
            setTab(page.name);
          },
        }))}
        dense={dense}
        activeTabName={tab}
        className={navClassName}
        style={navStyle}
        actions={actions}
      />
      <div className={pageContainerClassName} style={pageContainerStyle}>
        {currentPage.page}
      </div>
    </div>
  );
};

export default TabPages;
