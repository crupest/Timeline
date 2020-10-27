import React from "react";
import { useTranslation } from "react-i18next";
import { LinkContainer } from "react-router-bootstrap";
import { Navbar, Nav } from "react-bootstrap";

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
          <LinkContainer to="/settings">
            <Nav.Link>{t("nav.settings")}</Nav.Link>
          </LinkContainer>

          <LinkContainer to="/about">
            <Nav.Link>{t("nav.about")}</Nav.Link>
          </LinkContainer>

          {isAdministrator && (
            <LinkContainer to="/admin">
              <Nav.Link>Administration</Nav.Link>
            </LinkContainer>
          )}
        </Nav>
        <Nav className="ml-auto mr-2">
          {user != null ? (
            <LinkContainer to={`/users/${user.username}`}>
              <BlobImage
                className="avatar small rounded-circle bg-white"
                blob={avatar}
              />
            </LinkContainer>
          ) : (
            <LinkContainer to="/login">
              <Nav.Link>{t("nav.login")}</Nav.Link>
            </LinkContainer>
          )}
        </Nav>
      </Navbar.Collapse>
    </Navbar>
  );
};

export default AppBar;
