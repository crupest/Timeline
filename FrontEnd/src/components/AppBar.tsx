import { useState } from "react";
import classnames from "classnames";
import { Link, NavLink } from "react-router-dom";

import { useUser } from "~src/services/user";

import { I18nText, useC } from "./common";
import { useMobile } from "./hooks";
import TimelineLogo from "./TimelineLogo";
import { IconButton } from "./button";
import UserAvatar from "./user/UserAvatar";

import "./AppBar.css";

function AppBarNavLink({
  link,
  className,
  label,
  onClick,
  children,
}: {
  link: string;
  className?: string;
  label?: I18nText;
  onClick?: () => void;
  children?: React.ReactNode;
}) {
  if (label != null && children != null) {
    throw new Error("AppBarNavLink: label and children cannot be both set");
  }

  const c = useC();

  return (
    <NavLink
      to={link}
      className={({ isActive }) => classnames(className, isActive && "active")}
      onClick={onClick}
    >
      {children}
      {label && c(label)}
    </NavLink>
  );
}

export default function AppBar() {
  const isMobile = useMobile();

  const [isCollapse, setIsCollapse] = useState<boolean>(true);
  const collapse = isMobile ? () => setIsCollapse(true) : undefined;
  const toggleCollapse = () => setIsCollapse(!isCollapse);

  const user = useUser();
  const hasAdministrationPermission = user && user.hasAdministrationPermission;

  return (
    <nav
      className={classnames(
        isMobile ? "mobile" : "desktop",
        "app-bar",
        isCollapse || "expand",
      )}
    >
      <Link to="/" className="app-bar-brand" onClick={collapse}>
        <TimelineLogo className="app-bar-brand-icon" />
        Timeline
      </Link>

      <div className="app-bar-link-area">
        <AppBarNavLink
          link="/settings"
          label="nav.settings"
          onClick={collapse}
        />
        <AppBarNavLink link="/about" label="nav.about" onClick={collapse} />
        {hasAdministrationPermission && (
          <AppBarNavLink
            link="/admin"
            label="nav.administration"
            onClick={collapse}
          />
        )}
      </div>

      <div className="app-bar-space" />

      <div className="app-bar-user-area">
        {user != null ? (
          <AppBarNavLink link="/" className="app-bar-avatar" onClick={collapse}>
            <UserAvatar username={user.username} />
          </AppBarNavLink>
        ) : (
          <AppBarNavLink link="/login" label="nav.login" onClick={collapse} />
        )}
      </div>

      {isMobile && (
        <IconButton
          icon="list"
          color="light"
          className="toggler"
          onClick={toggleCollapse}
        />
      )}
    </nav>
  );
}
