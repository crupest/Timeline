import React from "react";

import { I18nText } from "@/common";

export interface Tab {
  name: string;
  text: I18nText;
  link?: string;
  onClick?: () => void;
}

export interface TabsProps {
  activeTabName?: string;
  tabs: Tab[];
}

export default function Tabs(props: TabsProps): React.ReactElement | null {
  return <div></div>;
}
