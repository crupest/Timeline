import React from "react";
import { Nav } from "react-bootstrap";
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
      <Nav variant="tabs" className={navClassName} style={navStyle}>
        {pages.map((page) => (
          <Nav.Item key={page.id}>
            <Nav.Link
              active={tab === page.id}
              onClick={() => {
                setTab(page.id);
              }}
            >
              {convertI18nText(page.tabText, t)}
            </Nav.Link>
          </Nav.Item>
        ))}
        {actions != null && (
          <div className="ms-auto cru-tab-pages-action-area">{actions}</div>
        )}
      </Nav>
      <div className={pageContainerClassName} style={pageContainerStyle}>
        {currentPage.page}
      </div>
    </div>
  );
};

export default TabPages;
