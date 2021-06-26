import React from "react";
import { useTranslation } from "react-i18next";

import { convertI18nText, I18nText, UiLogicError } from "@/common";

export interface TabPage {
  id: string;
  tabText: I18nText;
  page: React.ReactNode;
}

export interface TabPagesProps {
  pages: TabPage[];
  actions?: React.ReactNode;
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
  className,
  style,
  navClassName,
  navStyle,
  pageContainerClassName,
  pageContainerStyle,
}) => {
  // TODO:

  if (pages.length === 0) {
    throw new UiLogicError("Page list can't be empty.");
  }

  const { t } = useTranslation();

  const [tab, setTab] = React.useState<string>(pages[0].id);

  const currentPage = pages.find((p) => p.id === tab);

  if (currentPage == null) {
    throw new UiLogicError("Current tab value is bad.");
  }

  return (
    <div className={className} style={style}>
      <div className={pageContainerClassName} style={pageContainerStyle}>
        {currentPage.page}
      </div>
    </div>
  );
};

export default TabPages;
