import React, { Fragment } from "react";
import {
  Redirect,
  Route,
  Switch,
  useRouteMatch,
  useHistory,
} from "react-router";
import { Nav } from "react-bootstrap";

import { UserWithToken } from "@/services/user";

import UserAdmin from "./UserAdmin";

interface AdminProps {
  user: UserWithToken;
}

const Admin: React.FC<AdminProps> = (props) => {
  const match = useRouteMatch();
  const history = useHistory();
  type TabNames = "users" | "more";

  const tabName = history.location.pathname.replace(match.path + "/", "");

  function toggle(newTab: TabNames): void {
    history.push(`${match.url}/${newTab}`);
  }

  const createRoute = (
    name: string,
    body: React.ReactNode
  ): React.ReactNode => {
    return (
      <Route path={`${match.path}/${name}`}>
        <div style={{ height: 56 }} className="flex-fix-length" />
        <Nav variant="tabs">
          <Nav.Item>
            <Nav.Link
              active={tabName === "users"}
              onClick={() => {
                toggle("users");
              }}
            >
              Users
            </Nav.Link>
          </Nav.Item>
          <Nav.Item>
            <Nav.Link
              active={tabName === "more"}
              onClick={() => {
                toggle("more");
              }}
            >
              More
            </Nav.Link>
          </Nav.Item>
        </Nav>
        {body}
      </Route>
    );
  };

  return (
    <Fragment>
      <Switch>
        <Redirect from={match.path} to={`${match.path}/users`} exact />
        {createRoute("users", <UserAdmin user={props.user} />)}
        {createRoute("more", <div>More Page Works</div>)}
      </Switch>
    </Fragment>
  );
};

export default Admin;
