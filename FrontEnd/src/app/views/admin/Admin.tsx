import React, { Fragment } from "react";
import { Redirect, Route, Switch, useRouteMatch, match } from "react-router";

import { AuthUser } from "@/services/user";

import UserAdmin from "./UserAdmin";

interface AdminProps {
  user: AuthUser;
}

const Admin: React.FC<AdminProps> = ({ user }) => {
  const match = useRouteMatch();

  return (
    <Fragment>
      <Switch>
        <Redirect from={match.path} to={`${match.path}/users`} exact />
        <Route path={`${match.path}/:name`}>
          {(p) => {
            const match = p.match as match<{ name: string }>;
            const name = match.params["name"];
            if (name === "users") {
              return <UserAdmin user={user} />;
            }
          }}
        </Route>
      </Switch>
    </Fragment>
  );
};

export default Admin;
