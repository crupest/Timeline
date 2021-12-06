import React from "react";
import classnames from "classnames";
import { useTranslation } from "react-i18next";
import { Link, NavLink } from "react-router-dom";
import { useMediaQuery } from "react-responsive";

import { useUser } from "@/services/user";

import TimelineLogo from "./TimelineLogo";
import UserAvatar from "./user/UserAvatar";

import "./AppBar.css";

const AppBar: React.FC = (_) => {
  const { t } = useTranslation();

  const user = useUser();
  const hasAdministrationPermission = user && user.hasAdministrationPermission;

  const isSmallScreen = useMediaQuery({ maxWidth: 576 });

  const [expand, setExpand] = React.useState<boolean>(false);
  const collapse = (): void => setExpand(false);
  const toggleExpand = (): void => setExpand(!expand);

  const createLink = (
    link: string,
    label: React.ReactNode,
    className?: string
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
          !expand && "app-bar-collapse"
        )}
      >
        <div className="app-bar-link-area">
          {createLink("/settings", t("nav.settings"))}
          {createLink("/about", t("nav.about"))}
          {hasAdministrationPermission &&
            createLink("/admin", t("nav.administration"))}
        </div>

        <div className="app-bar-user-area">
          {user != null
            ? createLink(
                "/",
                <UserAvatar
                  username={user.username}
                  className="cru-avatar small cru-round cursor-pointer ml-auto"
                />,
                "app-bar-avatar"
              )
            : createLink("/login", t("nav.login"))}
        </div>
      </div>
    </nav>
  );
};

export default AppBar;
