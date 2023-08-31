import { ReactNode, useState } from "react";
import classNames from "classnames";

import { Text, UiLogicError } from "../common";

import Tabs from "./TabBar";

import "./TabPages.css";

interface TabPage {
  name: string;
  text: Text;
  page: ReactNode;
}

interface TabPagesProps {
  pages: TabPage[];
  actions?: ReactNode;
  dense?: boolean;
  className?: string;
  tabBarClassName?: string;
  pageContainerClassName?: string;
}

export default function TabPages({
  pages,
  actions,
  dense,
  className,
  tabBarClassName,
  pageContainerClassName,
}: TabPagesProps) {
  const [tab, setTab] = useState<string>(pages[0].name);

  const currentPage = pages.find((p) => p.name === tab);

  if (currentPage == null) throw new UiLogicError();

  return (
    <div className={className}>
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
        className={tabBarClassName}
        actions={actions}
      />
      <div
        className={classNames("cru-tab-page-container", pageContainerClassName)}
      >
        {currentPage.page}
      </div>
    </div>
  );
}
