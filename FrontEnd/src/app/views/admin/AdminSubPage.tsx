import React from "react";
import { Container, Nav } from "react-bootstrap";
import { useHistory, useRouteMatch } from "react-router";

const AdminSubPage: React.FC = ({ children }) => {
  const match = useRouteMatch<{ name: string }>();
  const history = useHistory();

  const name = match.params.name;

  function toggle(newTab: string): void {
    history.push(`/admin/${newTab}`);
  }

  return (
    <Container>
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
      </Nav>
      {children}
    </Container>
  );
};

export default AdminSubPage;
