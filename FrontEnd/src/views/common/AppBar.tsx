import * as React from "react";
import classnames from "classnames";
import { Link, NavLink } from "react-router-dom";
import { useMediaQuery } from "react-responsive";

import { I18nText, useC, useMobile } from "./common";
import { useUser } from "@/services/user";

import TimelineLogo from "./TimelineLogo";
import UserAvatar from "./user/UserAvatar";

import "./AppBar.css";

function AppBarNavLink({
  link,
  className,
  label,
  children,
}: {
  link: string;
  className?: string;
  label?: I18nText;
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
    >
      {children != null ? children : c(label)}
    </NavLink>
  );
}

function DesktopAppBar() {
  const user = useUser();
  const hasAdministrationPermission = user && user.hasAdministrationPermission;

  return (
    <nav className="desktop app-bar">
      <Link to="/" className="app-bar-brand active">
        <TimelineLogo className="app-bar-brand-icon" />
        Timeline
      </Link>
      <div className="app-bar-main-area">
        <div className="app-bar-link-area">
          <AppBarNavLink link="/settings" label="nav.settings" />
          <AppBarNavLink link="/about" label="nav.about" />
          {hasAdministrationPermission && (
            <AppBarNavLink link="/admin" label="nav.administration" />
          )}
        </div>

        <div className="app-bar-user-area">
          {user != null ? (
            <AppBarNavLink link="/" className="app-bar-avatar">
              <UserAvatar
                username={user.username}
                className="cru-avatar small cru-round cursor-pointer ml-auto"
              />
            </AppBarNavLink>
          ) : (
            <AppBarNavLink link="/login" label="nav.login" />
          )}
        </div>
      </div>
    </nav>
  );
}

// TODO: Go make this!
function MobileAppBar() {
  const c = useC();

  const user = useUser();
  const hasAdministrationPermission = user && user.hasAdministrationPermission;

  const isSmallScreen = useMediaQuery({ maxWidth: 576 });

  const [expand, setExpand] = React.useState<boolean>(false);
  const collapse = (): void => setExpand(false);
  const toggleExpand = (): void => setExpand(!expand);

  const createLink = (
    link: string,
    label: React.ReactNode,
    className?: string,
  ): React.ReactNode => (
    <NavLink
      to={link}
      onClick={collapse}
      className={({ isActive }) => classnames(className, isActive && "active")}
    >
      {label}
    </NavLink>
  );

  return (
    <nav className={classnames("app-bar", isSmallScreen && "small-screen")}>
      <Link to="/" className="app-bar-brand active">
        <TimelineLogo className="app-bar-brand-icon" />
        Timeline
      </Link>

      {isSmallScreen && (
        <i className="bi-list app-bar-toggler" onClick={toggleExpand} />
      )}

      <div
        className={classnames(
          "app-bar-main-area",
          !expand && "app-bar-collapse",
        )}
      >
        <div className="app-bar-link-area">
          {createLink("/settings", c("nav.settings"))}
          {createLink("/about", c("nav.about"))}
          {hasAdministrationPermission &&
            createLink("/admin", c("nav.administration"))}
        </div>

        <div className="app-bar-user-area">
          {user != null
            ? createLink(
                "/",
                <UserAvatar
                  username={user.username}
                  className="cru-avatar small cru-round cursor-pointer ml-auto"
                />,
                "app-bar-avatar",
              )
            : createLink("/login", c("nav.login"))}
        </div>
      </div>
    </nav>
  );
}

export default function AppBar() {
  const isMobile = useMobile();
  return isMobile ? <MobileAppBar /> : <DesktopAppBar />;
}
