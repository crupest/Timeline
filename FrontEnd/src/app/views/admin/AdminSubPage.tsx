import React from "react";
import { Nav } from "react-bootstrap";
import { useHistory, useRouteMatch } from "react-router";

const AdminSubPage: React.FC = ({ children }) => {
  const match = useRouteMatch<{ name: string }>();
  const history = useHistory();

  const name = match.params.name;

  function toggle(newTab: string): void {
    history.push(`/admin/${newTab}`);
  }

  return (
    <>
      <Nav variant="tabs">
        <Nav.Item>
          <Nav.Link
            active={name === "users"}
            onClick={() => {
              toggle("users");
            }}
          >
            Users
          </Nav.Link>
        </Nav.Item>
        <Nav.Item>
          <Nav.Link
            active={name === "more"}
            onClick={() => {
              toggle("more");
            }}
          >
            More
          </Nav.Link>
        </Nav.Item>
      </Nav>
      {children}
    </>
  );
};

export default AdminSubPage;
