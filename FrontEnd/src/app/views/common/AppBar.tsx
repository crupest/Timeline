import React from "react";
import { useTranslation } from "react-i18next";
import { LinkContainer } from "react-router-bootstrap";
import { Navbar, Nav } from "react-bootstrap";
import { NavLink } from "react-router-dom";

import { useUser, useAvatar } from "@/services/user";

import TimelineLogo from "./TimelineLogo";
import BlobImage from "./BlobImage";

const AppBar: React.FC = (_) => {
  const user = useUser();
  const avatar = useAvatar(user?.username);

  const { t } = useTranslation();

  const hasAdministrationPermission = user && user.hasAdministrationPermission;

  const [expand, setExpand] = React.useState<boolean>(false);
  const collapse = (): void => setExpand(false);
  const toggleExpand = (): void => setExpand(!expand);

  return (
    <Navbar
      bg="primary"
      variant="dark"
      expand="md"
      fixed="top"
      expanded={expand}
    >
      <LinkContainer to="/" onClick={collapse}>
        <Navbar.Brand className="d-flex align-items-center">
          <TimelineLogo style={{ height: "1em" }} />
          Timeline
        </Navbar.Brand>
      </LinkContainer>

      <Navbar.Toggle onClick={toggleExpand} />
      <Navbar.Collapse>
        <Nav className="mr-auto">
          <NavLink
            to="/settings"
            className="nav-link"
            activeClassName="active"
            onClick={collapse}
          >
            {t("nav.settings")}
          </NavLink>
          <NavLink
            to="/about"
            className="nav-link"
            activeClassName="active"
            onClick={collapse}
          >
            {t("nav.about")}
          </NavLink>

          {hasAdministrationPermission && (
            <NavLink
              to="/admin"
              className="nav-link"
              activeClassName="active"
              onClick={collapse}
            >
              {t("nav.administration")}
            </NavLink>
          )}
        </Nav>
        <Nav className="ml-auto mr-2 align-items-center">
          {user != null ? (
            <LinkContainer to={`/users/${user.username}`}>
              <BlobImage
                className="avatar small rounded-circle bg-white cursor-pointer ml-auto"
                onClick={collapse}
                blob={avatar}
              />
            </LinkContainer>
          ) : (
            <NavLink
              to="/login"
              className="nav-link"
              activeClassName="active"
              onClick={collapse}
            >
              {t("nav.login")}
            </NavLink>
          )}
        </Nav>
      </Navbar.Collapse>
    </Navbar>
  );
};

export default AppBar;
