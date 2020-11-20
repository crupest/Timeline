import React from "react";
import { Nav } from "react-bootstrap";
import { useHistory, useRouteMatch } from "react-router";

const AdminNav: React.FC = () => {
  const match = useRouteMatch<{ name: string }>();
  const history = useHistory();

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
          Users
        </Nav.Link>
      </Nav.Item>
      <Nav.Item>
        <Nav.Link
          active={name === "highlighttimelines"}
          onClick={() => {
            toggle("highlighttimelines");
          }}
        >
          Highlight Timelines
        </Nav.Link>
      </Nav.Item>
    </Nav>
  );
};

export default AdminNav;
