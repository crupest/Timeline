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

  const isAdministrator = user && user.administrator;

  return (
    <Navbar bg="primary" variant="dark" expand="md" sticky="top">
      <LinkContainer to="/">
        <Navbar.Brand className="d-flex align-items-center">
          <TimelineLogo style={{ height: "1em" }} />
          Timeline
        </Navbar.Brand>
      </LinkContainer>

      <Navbar.Toggle />
      <Navbar.Collapse>
        <Nav className="mr-auto">
          <NavLink to="/settings" className="nav-link" activeClassName="active">
            {t("nav.settings")}
          </NavLink>
          <NavLink to="/about" className="nav-link" activeClassName="active">
            {t("nav.about")}
          </NavLink>

          {isAdministrator && (
            <NavLink to="/admin" className="nav-link" activeClassName="active">
              Administration
            </NavLink>
          )}
        </Nav>
        <Nav className="ml-auto mr-2">
          {user != null ? (
            <LinkContainer to={`/users/${user.username}`}>
              <BlobImage
                className="avatar small rounded-circle bg-white cursor-pointer"
                blob={avatar}
              />
            </LinkContainer>
          ) : (
            <NavLink to="/login" className="nav-link" activeClassName="active">
              {t("nav.login")}
            </NavLink>
          )}
        </Nav>
      </Navbar.Collapse>
    </Navbar>
  );
};

export default AppBar;
