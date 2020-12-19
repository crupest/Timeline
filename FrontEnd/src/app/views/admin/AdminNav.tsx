import React from "react";
import { Nav } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import { useHistory, useRouteMatch } from "react-router";

const AdminNav: React.FC = () => {
  const match = useRouteMatch<{ name: string }>();
  const history = useHistory();

  const { t } = useTranslation();

  const name = match.params.name;

  function toggle(newTab: string): void {
    history.push(`/admin/${newTab}`);
  }

  return (
    <Nav variant="tabs" className="my-2">
      <Nav.Item>
        <Nav.Link
          active={name === "users"}
          onClick={() => {
            toggle("users");
          }}
        >
          {t("admin:nav.users")}
        </Nav.Link>
      </Nav.Item>
      <Nav.Item>
        <Nav.Link
          active={name === "more"}
          onClick={() => {
            toggle("more");
          }}
        >
          {t("admin:nav.more")}
        </Nav.Link>
      </Nav.Item>
    </Nav>
  );
};

export default AdminNav;
