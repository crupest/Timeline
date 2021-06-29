import React from "react";

import Menu, { MenuItems } from "./Menu";

export interface PopupMenuProps {
  items: MenuItems;
  children: React.ReactElement;
}

export const PopupMenu: React.FC<PopupMenuProps> = ({ items, children }) => {
  const [show, setShow] = React.useState<boolean>(false);
  const toggle = (): void => setShow(!show);

  // TODO:

  return <Menu items={items} onItemClicked={() => setShow(false)} />;
};
